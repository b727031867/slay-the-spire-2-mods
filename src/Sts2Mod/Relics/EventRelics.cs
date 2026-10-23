using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Pools;

namespace Sts2Mod.Relics;

public interface IEventComboRelic
{
    Task OnReward(PlayerChoiceContext context, ComboKind kind);
}

[Pool(typeof(TravelerEventRelicPool))]
public abstract class TravelerEventRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override string PackedIconPath => $"res://Sts2Mod/art/events/{Id.Entry}.svg";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;
}

[CustomID("TC_R08")]
public sealed class WallPiercingNail : TravelerEventRelic
{
    private bool _spent;
    public override Task BeforeCombatStart() { _spent = false; return Task.CompletedTask; }
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    { if (player == Owner) _spent = false; return Task.CompletedTask; }
    public async Task OnHit(PlayerChoiceContext context, Creature target)
    {
        if (_spent || IsMelted) return;
        _spent = true;
        Flash();
        if (target.IsAlive) await PowerCmd.Apply<VulnerablePower>(context, target, 1, Owner.Creature, null);
    }
}

[CustomID("TC_R09")]
public sealed class EchoSpool : TravelerEventRelic, IEventComboRelic
{
    private bool _spent;
    public override Task BeforeCombatStart() { _spent = false; return Task.CompletedTask; }
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    { if (player == Owner) _spent = false; return Task.CompletedTask; }
    public async Task OnReward(PlayerChoiceContext context, ComboKind kind)
    {
        if (_spent || IsMelted || kind != ComboKind.WindRock) return;
        var enemies = Owner.Creature.CombatState!.HittableEnemies.Where(e => e.IsAlive).ToArray();
        if (enemies.Length == 0) return;
        _spent = true;
        Flash();
        await PowerCmd.Apply<WeakPower>(context, Owner.RunState.Rng.CombatCardGeneration.NextItem(enemies)!, 1, Owner.Creature, null);
    }
}

[CustomID("TC_R10")]
public sealed class ReverseWatch : TravelerEventRelic;

[CustomID("TC_R11")]
public sealed class OutsideLetter : TravelerEventRelic, IEventComboRelic
{
    private int _kinds;
    public override Task BeforeCombatStart() { _kinds = 0; return Task.CompletedTask; }
    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (!IsMelted && player == Owner && player.PlayerCombatState!.TurnNumber == 1)
        {
            Flash();
            await CreatureCmd.Damage(context, Owner.Creature, 2, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        }
    }
    public async Task OnReward(PlayerChoiceContext context, ComboKind kind)
    {
        if (IsMelted || _kinds == 7 || kind == ComboKind.None) return;
        _kinds |= 1 << ((int)kind - 1);
        if (_kinds == 7) { Flash(); await CreatureCmd.Heal(Owner.Creature, 3); }
    }
}

[CustomID("TC_R12")]
public class AddressCorrection : TravelerEventRelic
{
    protected virtual IEnumerable<CardModel> Candidates => ModelDb.AllCards.OfType<ElementCardModel>()
        .Where(c => c.CardElement != Element.None && TravelerContent.RewardCardTypes.Contains(c.GetType()));
    public override decimal ModifyHandDraw(Player player, decimal count) => !IsMelted && player == Owner
        && player.PlayerCombatState!.TurnNumber == 1 ? Math.Max(0, count - 1) : count;
    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (IsMelted || player != Owner || player.PlayerCombatState!.TurnNumber != 1) return;
        var pool = Candidates.ToArray();
        if (pool.Length == 0) return;
        var card = Owner.Creature.CombatState!.CreateCard(Owner.RunState.Rng.CombatCardGeneration.NextItem(pool)!, Owner);
        CardCmd.ApplyKeyword(card, CardKeyword.Ethereal);
        card.EnergyCost.SetThisTurn(0);
        card.SetStarCostThisTurn(0);
        await CardPileCmd.Add(card, PileType.Hand);
        Flash();
    }
}

[CustomID("TC_R13")]
public sealed class UnclaimedMail : TravelerEventRelic;

[CustomID("TC_R14")]
public sealed class CompanionLetter : TravelerEventRelic
{
    private bool _healed;
    public override string PackedIconPath => "res://Sts2Mod/art/events/TC_R11.svg";
    public override Task BeforeCombatStart() { _healed = false; return Task.CompletedTask; }
    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (IsMelted || player != Owner || player.PlayerCombatState!.TurnNumber != 1) return;
        Flash();
        await CreatureCmd.Damage(context, Owner.Creature, 2, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
    }
    public override async Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (IsMelted || _healed || side != Owner.Creature.Side || !participants.Contains(Owner.Creature)
            || !Owner.Creature.IsAlive || Owner.PlayerCombatState!.TurnNumber != 3) return;
        _healed = true;
        Flash();
        await CreatureCmd.Heal(Owner.Creature, 3);
    }
}

[CustomID("TC_R15")]
public sealed class CompanionAddress : AddressCorrection
{
    public override string PackedIconPath => "res://Sts2Mod/art/events/TC_R12.svg";
    protected override IEnumerable<CardModel> Candidates => Owner.Character.CardPool.GetUnlockedCards(Owner.UnlockState, 0)
        .Where(c => c.Rarity is CardRarity.Common or CardRarity.Uncommon
            && !c.EnergyCost.CostsX && !c.HasStarCostX && c.EnergyCost.Canonical >= 0 && !c.Keywords.Contains(CardKeyword.Unplayable));
}
