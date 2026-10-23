using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Sts2Mod.Pools;
using Sts2Mod.Powers;

namespace Sts2Mod.Cards;

[CustomID("TC_C10"), Pool(typeof(TravelerCardPool))]
public sealed class ElementResonance : TravelerPowerCard<ElementResonancePower>
{
    public ElementResonance() : base(2, CardRarity.Rare, 1m) { }
    public override string CustomPortraitPath => "res://Sts2Mod/art/element_resonance.png";
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars["PowerAmount"].UpgradeValueBy(1);
    }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var power = await PowerCmd.Apply<ElementResonancePower>(context, Owner.Creature, 1, Owner.Creature, this);
        power?.AddCopy(DynamicVars["PowerAmount"].IntValue);
    }
}
