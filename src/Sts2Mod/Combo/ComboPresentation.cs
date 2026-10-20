using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;
using Sts2Mod.Relics;

namespace Sts2Mod.Combo;

public static class ComboPresentation
{
    public static bool HasPaimon(Player? player) => player?.Relics.Any(r => r is PaimonCheer { IsMelted: false }) == true;
    public static int Reward(ComboKind kind, bool doubled) => (kind == ComboKind.LightningWind ? 1 : 4) * (doubled ? 2 : 1);

    public static bool InHand(ElementCardModel card) => card.IsMutable && card.Pile?.Type == PileType.Hand
        && card.Owner?.PlayerCombatState is not null && CombatManager.Instance.IsInProgress && !CombatManager.Instance.IsEnding;

    public static LocString Preview(ElementCardModel card)
    {
        string key;
        var power = InHand(card) ? card.Owner.Creature.GetPower<ComboPower>() : null;
        var kind = power?.State.Preview(card.CardElement) ?? ComboKind.None;
        if (power is null) key = "generic";
        else if (RunManager.Instance.ActionQueueSynchronizer.CombatState != ActionSynchronizerCombatState.PlayPhase) key = "waiting";
        else if (!card.CanPlay()) key = "unplayable";
        else if (power.State.Remaining == 0) key = "exhausted";
        else if (kind == ComboKind.None) key = "record";
        else key = kind switch
        {
            ComboKind.WindRock => "block",
            ComboKind.LightningWind => "draw",
            ComboKind.RockLightning when card.Type == CardType.Skill && card.TargetType != TargetType.AnyEnemy => "randomAttack",
            _ => "attack"
        };
        var text = new LocString("powers", "TC_PREVIEW." + key);
        text.Add("Element", new LocString("powers", "TC_ELEMENT_" + card.CardElement.ToString().ToUpperInvariant()));
        text.Add("Remaining", power?.State.Remaining ?? ComboState.Limit);
        text.Add("Bonus", Reward(kind, card.IsMutable && HasPaimon(card.Owner)));
        return text;
    }

    public static LocString Trigger(ComboKind kind, bool doubled, int? actualDraw = null)
    {
        var key = kind switch { ComboKind.WindRock => "trigger", ComboKind.RockLightning => "rockLightningTrigger", _ => "lightningWindTrigger" };
        if (kind == ComboKind.LightningWind && actualDraw is { } actual && actual < Reward(kind, doubled)) key = "limitedDrawTrigger";
        var text = new LocString("powers", "TC_COMBO." + key);
        text.Add("Bonus", Reward(kind, doubled));
        text.Add("Actual", actualDraw ?? Reward(kind, doubled));
        text.Add("Paimon", new LocString("powers", doubled ? "TC_COMBO.paimonSuffix" : "TC_COMBO.noSuffix"));
        return text;
    }
}
