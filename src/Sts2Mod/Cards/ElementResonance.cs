using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Sts2Mod.Pools;
using Sts2Mod.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace Sts2Mod.Cards;

[CustomID("TC_C10"), Pool(typeof(TravelerCardPool))]
public sealed class ElementResonance : TravelerPowerCard<ElementResonancePower>
{
    public ElementResonance() : base(2, CardRarity.Rare, 1m) { }
    public override string CustomPortraitPath => "res://Sts2Mod/art/element_resonance.png";
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("RewardThreshold", CurrentUpgradeLevel > 0 ? 1 : 2);
    }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (CurrentUpgradeLevel > 0)
            await PowerCmd.Apply<FirstElementResonancePower>(context, Owner.Creature,
                DynamicVars["PowerAmount"].BaseValue, Owner.Creature, this);
        else
            await base.OnPlay(context, play);
    }
}
