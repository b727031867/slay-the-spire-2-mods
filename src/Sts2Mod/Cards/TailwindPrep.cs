using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Sts2Mod.Combo;

namespace Sts2Mod.Cards;

[CustomID("TC_C04"), Pool(typeof(IroncladCardPool))]
public sealed class TailwindPrep : CustomCardModel
{
    public TailwindPrep() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }
    public override string CustomPortraitPath => "res://Sts2Mod/art/tailwind.svg";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = ComboPower.Reserve(this, context, play, Element.Wind);
        await CardPileCmd.Draw(context, DynamicVars.Cards.BaseValue, Owner);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}
