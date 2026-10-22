using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2Mod.Powers;

[CustomID("TC_ROCK_HEART")]
public sealed class RockHeartPower : TravelerAbilityPower
{
    public override string CustomPackedIconPath => "res://Sts2Mod/art/rock_heart.png";
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player || !CanReward) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
