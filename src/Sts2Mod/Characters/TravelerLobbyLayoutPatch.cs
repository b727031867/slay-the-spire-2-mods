using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;

namespace Sts2Mod.Characters;

/// <summary>FlowContainer owns row placement; do not replay a stale pre-layout shake origin.</summary>
[HarmonyPatch]
internal static class TravelerLobbyLayoutPatch
{
    private static IEnumerable<MethodBase> TargetMethods() => AccessTools.GetDeclaredMethods(typeof(NRemoteLobbyPlayer))
        .Where(method=>method.Name==nameof(NRemoteLobbyPlayer.OnPlayerChanged));

    private static bool IsTraveler(NRemoteLobbyPlayer row) =>
        row.GetNodeOrNull<TextureRect>("%CharacterIcon")?.Texture?.ResourcePath
            == "res://Sts2Mod/character/head_icon.png";

    private static void Prefix(NRemoteLobbyPlayer __instance,out bool __state) => __state=IsTraveler(__instance);

    private static void Postfix(NRemoteLobbyPlayer __instance,bool __state)
    {
        if(!__state && !IsTraveler(__instance))return;
        // Native OnPlayerChanged has already updated labels, portrait and ready/disconnect indicators.
        // Its ScreenPunch caches Position before the container's deferred layout, possibly (0,0)
        // for multiple rows. Cancel only Traveler transitions, including switching away from Traveler.
        __instance.CancelShake();
        if(__instance.GetParent() is Container container)container.QueueSort();
    }
}
