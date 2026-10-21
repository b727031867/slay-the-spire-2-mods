using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Sts2Mod.Combo;

public abstract class ElementCardModel(int cost, CardType type, CardRarity rarity, TargetType target)
    : CustomCardModel(cost, type, rarity, target)
{
    public abstract Element CardElement { get; }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            // Reward/draft/library cards cannot be played now. Their native keyword
            // tips are sufficient; the full rulebook obscures adjacent choices.
            if (!ComboPresentation.InHand(this)) yield break;
            yield return new HoverTip(new LocString("powers", "TC_PREVIEW.title"), ComboPresentation.Preview(this));
        }
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("ComboPreview", ComboPresentation.InHand(this) ? "\n" + ComboPresentation.Preview(this).GetFormattedText() : "");
    }
}
