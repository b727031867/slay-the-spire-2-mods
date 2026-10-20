using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Sts2Mod.Combo;

namespace Sts2Mod.Powers;

public abstract class TravelerAbilityPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => "res://Sts2Mod/art/compass.svg";
    public override string CustomBigIconPath => CustomPackedIconPath;
    protected bool CanReward => Owner.IsAlive && CombatManager.Instance.IsInProgress
        && !CombatManager.Instance.IsEnding && Owner.CombatState?.Enemies.Any(e => e.IsAlive) == true;
    public virtual Task OnComboReward(PlayerChoiceContext context, ComboKind kind, int actualRewardNumber) => Task.CompletedTask;
}
