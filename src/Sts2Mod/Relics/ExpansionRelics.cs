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
using Sts2Mod.Combo;
using Sts2Mod.Pools;

namespace Sts2Mod.Relics;

[CustomID("TC_R04"), Pool(typeof(TravelerRelicPool))]
public sealed class TravelerMedicineChest : CustomRelicModel
{
    private bool _spent;
    public override RelicRarity Rarity => RelicRarity.Common;
    public override string PackedIconPath => "res://Sts2Mod/art/TC_R04.png";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;
    public override Task BeforeCombatStart() { _spent = false; return Task.CompletedTask; }
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    { if (player == Owner) _spent = false; return Task.CompletedTask; }
    public override async Task AfterCardExhausted(PlayerChoiceContext context, CardModel card, bool causedByEthereal)
    {
        if (IsMelted || _spent || card.Owner != Owner || card.Type is not (CardType.Status or CardType.Curse)) return;
        _spent = true;
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, 5, ValueProp.Unpowered, null);
    }
}

[CustomID("TC_R05"), Pool(typeof(TravelerRelicPool))]
public sealed class TricolorPrism : CustomRelicModel
{
    private bool _spent;
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override string PackedIconPath => "res://Sts2Mod/art/TC_R05.png";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;
    public override Task BeforeCombatStart() { _spent = false; return Task.CompletedTask; }
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    { if (player == Owner) _spent = false; return Task.CompletedTask; }
    public override Task BeforeCardPlayed(CardPlay play)
    {
        if (IsMelted || _spent || play.Card.Owner != Owner || !play.IsFirstInSeries) return Task.CompletedTask;
        _spent = true;
        CombatCardElements.Assigned.Set(play.Card, (int)Owner.RunState.Rng.CombatCardGeneration.NextItem(new[] { Element.Wind, Element.Rock, Element.Lightning }));
        Flash();
        return Task.CompletedTask;
    }
}

[CustomID("TC_R06"), Pool(typeof(TravelerRelicPool))]
public sealed class ExpeditionDrum : CustomRelicModel, IComboRewardRelic
{
    public bool Ready { get; private set; }
    private CardModel? _attack;
    public override RelicRarity Rarity => RelicRarity.Rare;
    public override string PackedIconPath => "res://Sts2Mod/art/TC_R06.png";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;
    public override bool ShowCounter => true;
    public override int DisplayAmount => IsMutable && Ready ? 1 : 0;
    private void Reset() { Ready = false; _attack = null; InvokeDisplayAmountChanged(); }
    public override Task BeforeCombatStart() { Reset(); return Task.CompletedTask; }
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    { if (player == Owner) Reset(); return Task.CompletedTask; }
    public override Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    { if (participants.Contains(Owner.Creature)) Reset(); return Task.CompletedTask; }
    public Task OnComboReward(PlayerChoiceContext context)
    {
        if (!IsMelted && Owner.Creature.GetPower<ComboPower>()?.State.RewardsThisTurn == 3)
        { Ready = true; Flash(); InvokeDisplayAmountChanged(); }
        return Task.CompletedTask;
    }
    public override Task BeforeCardPlayed(CardPlay play)
    {
        if (!IsMelted && Ready && play.Card.Owner == Owner && !play.IsAutoPlay && play.IsFirstInSeries
            && play.Card.Type == CardType.Attack && play.Card.TargetType == TargetType.AnyEnemy)
        { Ready = false; _attack = play.Card; InvokeDisplayAmountChanged(); }
        return Task.CompletedTask;
    }
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        => !IsMelted && props.HasFlag(ValueProp.Move) && cardSource is not null && cardSource.Owner == Owner
            && cardSource == _attack ? 8 : 0;
    public override Task AfterDamageGiven(PlayerChoiceContext context, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    { if (cardSource == _attack) _attack = null; return Task.CompletedTask; }
    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay play)
    { if (play.Card == _attack) _attack = null; return Task.CompletedTask; }
}

[CustomID("TC_R07"), Pool(typeof(TravelerRelicPool))]
public sealed class StatueBlessing : CustomRelicModel
{
    public bool Spent { get; private set; }
    public override RelicRarity Rarity => RelicRarity.Rare;
    public override string PackedIconPath => "res://Sts2Mod/art/TC_R07.png";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;
    public override Task BeforeCombatStart() { Spent = false; return Task.CompletedTask; }
    public override Task AfterCombatEnd(CombatRoom room) { Spent = false; return Task.CompletedTask; }
    public async Task OnComboTriggered(PlayerChoiceContext context)
    {
        if (IsMelted || Spent) return;
        Spent = true;
        Flash();
        await PowerCmd.Apply<PlatingPower>(context, Owner.Creature, 6, Owner.Creature, null);
    }
}
