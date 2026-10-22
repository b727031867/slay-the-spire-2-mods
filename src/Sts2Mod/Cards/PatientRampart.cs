using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Combo;
using Sts2Mod.Pools;

namespace Sts2Mod.Cards;

[CustomID("TC_C16"), Pool(typeof(TravelerCardPool))]
public sealed class PatientRampart : ElementCardModel
{
    public PatientRampart() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }
    public override Element CardElement => Element.Rock;
    public override string CustomPortraitPath => "res://Sts2Mod/art/patient_rampart.png";
    public override bool GainsBlock => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move)];
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}
