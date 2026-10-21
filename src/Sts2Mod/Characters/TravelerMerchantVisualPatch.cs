using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace Sts2Mod.Characters;

// BaseLib 3.4.7's factory-instance tracking does not survive the cached PackedScene
// conversion used by the shop. Its non-Spine guard therefore misses this instance.
// Identify only our own uniquely named art; vanilla and other mods keep their animations.
[HarmonyPatch]
internal static class TravelerMerchantVisualPatch
{
    internal static bool IsTraveler(NMerchantCharacter character) =>
        character.GetNodeOrNull<Sprite2D>("%TravelerMerchantArt")?.Texture?.ResourcePath
            == "res://Sts2Mod/character/body.png";

    [HarmonyPatch(typeof(NMerchantCharacter), nameof(NMerchantCharacter._Ready))]
    [HarmonyPrefix]
    private static bool Ready(NMerchantCharacter __instance) => !IsTraveler(__instance);

    [HarmonyPatch(typeof(NMerchantCharacter), nameof(NMerchantCharacter.PlayAnimation))]
    [HarmonyPrefix]
    private static bool PlayAnimation(NMerchantCharacter __instance) => !IsTraveler(__instance);
}
