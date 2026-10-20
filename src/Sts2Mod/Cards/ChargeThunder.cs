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

[CustomID("TC_C17"), Pool(typeof(TravelerCardPool))]
public sealed class ChargeThunder : ElementCardModel
{
    public ChargeThunder() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }
    public override Element CardElement => Element.Lightning;
    public override string CustomPortraitPath => "res://Sts2Mod/art/charge_thunder.png";
    public override bool GainsBlock => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4m, ValueProp.Move), new EnergyVar(1)];
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = ComboPower.Reserve(this, context, play, CardElement);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await PowerCmd.Apply<EnergyNextTurnPower>(context, Owner.Creature, DynamicVars.Energy.BaseValue, Owner.Creature, this);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}
