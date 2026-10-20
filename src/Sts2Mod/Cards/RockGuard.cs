using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Combo;

namespace Sts2Mod.Cards;

[CustomID("TC_C02"), Pool(typeof(IroncladCardPool))]
public sealed class RockGuard : ElementCardModel
{
    public RockGuard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }
    public override Element CardElement => Element.Rock;
    public override string CustomPortraitPath => "res://Sts2Mod/art/rock.png";
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Move)];
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var reservation = ComboPower.Reserve(this, choiceContext, cardPlay, Element.Rock);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await ComboPower.Resolve(this, choiceContext, cardPlay, reservation);
    }
}
