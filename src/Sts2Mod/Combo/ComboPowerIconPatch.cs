using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Sts2Mod.Combo;

// Keep the native status icon in sync, without mounting any custom combat panel.
[HarmonyPatch(typeof(NPower), "OnDisplayAmountChanged")]
internal static class ComboPowerIconPatch
{
    private static void Postfix(NPower __instance)
    {
        if (__instance.IsNodeReady() && __instance.Model is ComboPower power
            && __instance.GetNodeOrNull<TextureRect>("%Icon") is { } icon)
            icon.Texture = ComboIcons.ElementTexture(power.State.Last);
    }
}
