using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using Sts2Mod.Combo;
using Sts2Mod.Pools;

namespace Sts2Mod.Relics;

/// <summary>Orobas upgrade: draw per reward and combat Strength on the third trigger each turn.</summary>
[CustomID("TC_R03"), Pool(typeof(TravelerRelicPool))]
public sealed class UnboundJourneyCompass : CustomRelicModel, IComboRewardRelic
{
    private bool _strengthGranted;
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool IsStackable => false;
    public override string PackedIconPath => "res://Sts2Mod/art/TC_R03.png";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;

    public override Task BeforeCombatStart() { _strengthGranted = false; return Task.CompletedTask; }
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (player == Owner) _strengthGranted = false;
        return Task.CompletedTask;
    }

    public async Task OnComboTriggered(PlayerChoiceContext context)
    {
        if (_strengthGranted || IsMelted || !Owner.Creature.IsAlive
            || !CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding
            || Owner.Creature.GetPower<ComboPower>()?.State.Used != 3) return;
        _strengthGranted = true;
        Flash();
        await PowerCmd.Apply<StrengthPower>(context, Owner.Creature, 1m, Owner.Creature, null);
    }

    public async Task OnComboReward(PlayerChoiceContext context)
    {
        if (IsMelted || !Owner.Creature.IsAlive || !CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding
            || Owner.Creature.CombatState?.Enemies.Any(enemy => enemy.IsAlive) != true) return;
        Flash();
        var actual = (await CardPileCmd.Draw(context, 1m, Owner)).Count();
        GD.Print($"STS2MOD_UNBOUND_COMPASS draw={actual} requested=1 cooldown=none");
    }
}
