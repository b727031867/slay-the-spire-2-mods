using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Sts2Mod.Combo;

[HarmonyPatch]
internal static class ElementRequirementPatch
{
    private static MethodBase TargetMethod() => AccessTools.Method(
        AccessTools.TypeByName("MegaCrit.Sts2.Core.Entities.Cards.UnplayableReasonExtensions"), "GetPlayerDialogueLine");
    private static bool Prefix(UnplayableReason reason, AbstractModel? preventer, ref LocString? __result)
    {
        if (preventer is not ComboLifecycle || !reason.HasFlag(UnplayableReason.BlockedByHook)) return true;
        __result = new LocString("cards", "TC_C27.requiresElement");
        return false;
    }
}
