using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Sts2Mod.Combo;

namespace Sts2Mod.Powers;

[CustomID("TC_ELEMENT_RESONANCE")]
public sealed class ElementResonancePower : TravelerAbilityPower
{
    public override string CustomPackedIconPath => "res://Sts2Mod/art/element_resonance.png";
    public override async Task OnComboReward(PlayerChoiceContext context, ComboKind kind, int actualRewardNumber)
    {
        if (!CanReward || actualRewardNumber != 2) return;
        Flash();
        await PlayerCmd.GainEnergy(Amount, Owner.Player!);
    }
}
