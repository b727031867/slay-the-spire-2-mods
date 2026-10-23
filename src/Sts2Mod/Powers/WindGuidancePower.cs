using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Sts2Mod.Combo;

namespace Sts2Mod.Powers;

[CustomID("TC_WIND_GUIDANCE")]
public sealed class WindGuidancePower : TravelerAbilityPower
{
    public override string CustomPackedIconPath => "res://Sts2Mod/ui/powers/wind_guidance.png";
    private int _spentCopies;
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (player == Owner.Player) _spentCopies = 0;
        return Task.CompletedTask;
    }
    public override async Task OnComboReward(PlayerChoiceContext context, ComboKind kind, int actualRewardNumber)
    {
        if (!CanReward || kind is not (ComboKind.LightningWind or ComboKind.WindRock) || Amount <= _spentCopies) return;
        var draws = Amount - _spentCopies;
        _spentCopies = Amount; // Pre-reserve all active copies before awaiting native draw.
        Flash();
        await CardPileCmd.Draw(context, draws, Owner.Player!);
    }
}
