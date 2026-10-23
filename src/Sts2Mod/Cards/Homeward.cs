using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2Mod.Cards;

[CustomID("TC_CURSE_HOMEWARD"), Pool(typeof(Sts2Mod.Pools.TravelerEventCardPool))]
public sealed class Homeward() : CustomCardModel(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    public override string CustomPortraitPath => "res://Sts2Mod/art/events/Homeward.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];
    protected override Task OnPlay(PlayerChoiceContext context, CardPlay play) => Task.CompletedTask;
}
