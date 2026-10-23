using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Sts2Mod.Combo;

namespace Sts2Mod.Powers;

// Retain the legacy model ID; new cards use the ordered ElementResonancePower.
[CustomID("TC_FIRST_ELEMENT_RESONANCE")]
public sealed class FirstElementResonancePower : TravelerAbilityPower
{
    public override string CustomPackedIconPath => "res://Sts2Mod/art/element_resonance.png";
    public override async Task OnComboReward(PlayerChoiceContext context, ComboKind kind, int actualRewardNumber)
    {
        if (!CanReward || actualRewardNumber != 1) return;
        Flash();
        await PlayerCmd.GainEnergy(Amount, Owner.Player!);
    }
}
