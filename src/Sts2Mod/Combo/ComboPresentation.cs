using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;
using Sts2Mod.Relics;
using Sts2Mod.Powers;
using Sts2Mod.Cards;

namespace Sts2Mod.Combo;

public readonly record struct ComboVisualPreview(string Key, ComboKind Kind, Element Element, int Remaining, int Bonus)
{
    public bool HighlightsReward => Key is "block" or "draw" or "attack" or "randomAttack";
}

public static class ComboPresentation
{
    public static bool HasPaimon(Player? player) => player?.Relics.Any(r => r is PaimonCheer { IsMelted: false }) == true;
    public static int EchoBonus(Player? player) => player?.Creature.GetPower<ThunderEchoPower>()?.Amount ?? 0;
    public static int Reward(ComboKind kind, bool doubled, int echoAmount = 0)
    {
        var baseReward = kind == ComboKind.LightningWind ? 1 : 4;
        var echoBonus = echoAmount > 0 ? (kind == ComboKind.LightningWind ? 1 : Math.Min(echoAmount, 4)) : 0;
        return (baseReward + echoBonus) * (doubled ? 2 : 1);
    }

    public static bool InHand(ElementCardModel card) => card.IsMutable && card.Pile?.Type == PileType.Hand
        && card.Owner?.PlayerCombatState is not null && CombatManager.Instance.IsInProgress && !CombatManager.Instance.IsEnding;

    public static ComboVisualPreview VisualPreview(ElementCardModel card)
    {
        string key;
        var power = InHand(card) ? card.Owner.Creature.GetPower<ComboPower>() : null;
        var element = CombatCardElements.Get(card);
        var kind = power?.State.Preview(element) ?? ComboKind.None;
        if (power is null) key = "generic";
        else if (RunManager.Instance.ActionQueueSynchronizer.CombatState != ActionSynchronizerCombatState.PlayPhase
            || CombatManager.Instance.PlayerActionsDisabled || !CombatManager.Instance.IsPartOfPlayerTurn(card.Owner)) key = "waiting";
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
        return new(key, kind, element, power?.State.Remaining ?? ComboState.Limit,
            Reward(kind, card.IsMutable && HasPaimon(card.Owner), card.IsMutable ? EchoBonus(card.Owner) : 0));
    }

    public static LocString Preview(ElementCardModel card)
    {
        var preview = VisualPreview(card);
        var text = new LocString("powers", "TC_PREVIEW." + preview.Key);
        text.Add("Element", new LocString("powers", "TC_ELEMENT_" + preview.Element.ToString().ToUpperInvariant()));
        text.Add("Remaining", preview.Remaining);
        text.Add("Limit", card.IsMutable ? card.Owner.Creature.GetPower<ComboPower>()?.State.TurnLimit ?? ComboState.Limit : ComboState.Limit);
        text.Add("Bonus", preview.Bonus);
        return text;
    }

    public static LocString Trigger(ComboKind kind, bool doubled, int? actualDraw = null, int echoAmount = 0)
    {
        var key = kind switch { ComboKind.WindRock => "trigger", ComboKind.RockLightning => "rockLightningTrigger", _ => "lightningWindTrigger" };
        var reward = Reward(kind, doubled, echoAmount);
        if (kind == ComboKind.LightningWind && actualDraw is { } actual && actual < reward) key = "limitedDrawTrigger";
        var text = new LocString("powers", "TC_COMBO." + key);
        text.Add("Bonus", reward);
        text.Add("Actual", actualDraw ?? reward);
        text.Add("Paimon", new LocString("powers", doubled ? "TC_COMBO.paimonSuffix" : "TC_COMBO.noSuffix"));
        return text;
    }
}
