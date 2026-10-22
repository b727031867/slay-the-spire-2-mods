using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using HarmonyLib;
using System.Reflection;

namespace Sts2Mod.Combo;

public static class CombatCardElements
{
    // Native combat recovery replays synchronized actions; serialized cards also retain their instance mark.
    public static readonly SavedSpireField<CardModel, int> Assigned = new(() => 0, "TC_Element");
    internal static readonly SpireField<CardModel, bool> Chosen = new(() => false);
    internal static readonly SpireField<CardModel, int> Exhaustions = new(() => 0);
    public static Element Get(CardModel card, Element fallback = Element.None)
        => Assigned.Get(card) is > 0 and <= 3 ? (Element)Assigned.Get(card)
            : card is ElementCardModel elemental ? elemental.CardElement : fallback;
    internal static void Initialize() { _ = Assigned; }
}

[HarmonyPatch]
internal static class CombatCardDescriptionPatch
{
    private static MethodBase TargetMethod() => AccessTools.GetDeclaredMethods(typeof(CardModel))
        .Single(m => m.Name == "GetDescriptionForPile" && m.GetParameters().Length == 3);
    private static void Postfix(CardModel __instance, ref string __result)
    {
        if (!__instance.IsMutable || __instance.Pile?.IsCombatPile != true) return;
        if (CombatCardElements.Assigned.Get(__instance) != 0)
        {
            var text = new LocString("cards", "TC_PRISM.assignedElement");
            text.Add("Element", new LocString("powers", "TC_ELEMENT_" + CombatCardElements.Get(__instance).ToString().ToUpperInvariant()));
            __result += "\n" + text.GetFormattedText();
        }
        if (__instance.Type == CardType.Attack && __instance.TargetType == TargetType.AnyEnemy
            && __instance.Owner.Relics.Any(r => r is Sts2Mod.Relics.ExpeditionDrum { Ready: true, IsMelted: false }))
            __result += "\n" + new LocString("cards", "TC_DRUM.firstHit").GetFormattedText();
    }
}
