using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Sts2Mod.Combo;
using MegaCrit.Sts2.Core.Localization;

namespace Sts2Mod.Powers;

[CustomID("TC_ELEMENT_RESONANCE")]
public sealed class ElementResonancePower : TravelerAbilityPower
{
    public override string CustomPackedIconPath => "res://Sts2Mod/ui/powers/element_resonance.png";
    protected override object InitInternalData() => new List<int>();
    private List<int> EnergyOrder => GetInternalData<List<int>>();
    public void AddCopy(int energy) { EnergyOrder.Add(energy); InvokeDisplayAmountChanged(); }
    public override LocString Description
    {
        get
        {
            var description = new LocString("powers", "TC_ELEMENT_RESONANCE.description");
            description.Add("Amount", IsMutable ? Amount : 1);
            description.Add("EnergyOrder", IsMutable ? string.Join(" / ", EnergyOrder) : "1");
            return description;
        }
    }
    public override async Task OnComboReward(PlayerChoiceContext context, ComboKind kind, int actualRewardNumber)
    {
        if (!CanReward || actualRewardNumber < 1 || actualRewardNumber > EnergyOrder.Count) return;
        Flash();
        await PlayerCmd.GainEnergy(EnergyOrder[actualRewardNumber - 1], Owner.Player!);
    }
}
