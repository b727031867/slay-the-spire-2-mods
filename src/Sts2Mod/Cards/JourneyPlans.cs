using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Sts2Mod.Pools;

namespace Sts2Mod.Cards;

[CustomID("TC_C18"), Pool(typeof(TravelerCardPool))]
public sealed class JourneyPlans : CustomCardModel
{
    public JourneyPlans() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }
    public override string CustomPortraitPath => "res://Sts2Mod/art/journey_plans.svg";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var selected = await CardSelectCmd.FromHand(context, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, CurrentUpgradeLevel > 0 ? 0 : 1, DynamicVars.Cards.IntValue),
            card => card != this && !card.ShouldRetainThisTurn, this);
        foreach (var card in selected) card.GiveSingleTurnRetain();
    }
}
