using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Combo;

namespace Sts2Mod.Powers;

[CustomID("TC_THUNDER_ECHO")]
public sealed class ThunderEchoPower : TravelerAbilityPower
{
    public override string CustomPackedIconPath => "res://Sts2Mod/art/thunder_echo.png";
    public override async Task OnComboReward(PlayerChoiceContext context, ComboKind kind, int actualRewardNumber)
    {
        if (!CanReward || kind != ComboKind.RockLightning) return;
        Flash();
        await CreatureCmd.Damage(context, Owner.CombatState!.Enemies.Where(e => e.IsAlive).ToArray(),
            Amount, ValueProp.Unpowered, Owner);
    }
}
