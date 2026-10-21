using BaseLib.Utils;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using Sts2Mod.Combo;
using Sts2Mod.Pools;

namespace Sts2Mod.Relics;

internal interface IComboRewardRelic
{
    Task OnComboReward(PlayerChoiceContext context);
}

[CustomID("TC_R02"), Pool(typeof(TravelerRelicPool))]
public sealed class JourneyCompass : CustomRelicModel, IComboRewardRelic
{
    private bool _inCombat;
    private bool _strengthGranted;
    public bool SpentThisTurn => _strengthGranted;
    public override RelicModel GetUpgradeReplacement() => ModelDb.Relic<UnboundJourneyCompass>();
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool IsStackable => false;
    public override bool ShowCounter => true;
    public override int DisplayAmount => !IsMutable || !_inCombat || _strengthGranted ? 0
        : Math.Min(3, Owner.Creature.GetPower<ComboPower>()?.State.RewardsThisTurn ?? 0);
    public override string PackedIconPath => "res://Sts2Mod/art/compass.png";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;
    public override Task AfterObtained()
    {
        _inCombat = CombatManager.Instance.IsInProgress;
        Reset();
        Owner.Creature.GetPower<ComboPower>()?.RefreshLimit();
        return Task.CompletedTask;
    }
    public override Task BeforeCombatStart() { _inCombat = true; Reset(); return Task.CompletedTask; }
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (player == Owner) Reset();
        return Task.CompletedTask;
    }
    public override Task AfterCombatEnd(CombatRoom room) { _inCombat = false; Reset(); return Task.CompletedTask; }
    private void Reset()
    {
        _strengthGranted = false;
        InvokeDisplayAmountChanged();
    }
    public async Task OnComboReward(PlayerChoiceContext context)
    {
        if (!_inCombat || IsMelted || !Owner.Creature.IsAlive || _strengthGranted
            || Owner.Creature.GetPower<ComboPower>()?.State.RewardsThisTurn != 3)
        {
            InvokeDisplayAmountChanged();
            return;
        }
        _strengthGranted = true;
        InvokeDisplayAmountChanged();
        Flash();
        await PowerCmd.Apply<StrengthPower>(context, Owner.Creature, 1m, Owner.Creature, null);
    }
}
