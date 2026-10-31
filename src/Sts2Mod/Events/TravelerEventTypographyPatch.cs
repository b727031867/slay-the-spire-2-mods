using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace Sts2Mod.Events;

[HarmonyPatch(typeof(NEventOptionButton), nameof(NEventOptionButton._Ready))]
internal static class TravelerEventTypographyPatch
{
    private const string MaxSizeKey = "traveler_option_max_font";

    private static void Postfix(NEventOptionButton __instance)
    {
        if (__instance.Event is not TravelerEvent) return;
        var label = __instance.GetNode<MegaRichTextLabel>("%Text");
        label.SetMeta(MaxSizeKey, label.MaxFontSize);
        void Refresh() => Callable.From(() => Synchronize(__instance)).CallDeferred();
        label.Resized += Refresh;
        Refresh();
    }

    private static void Synchronize(NEventOptionButton button)
    {
        if (!GodotObject.IsInstanceValid(button) || !button.IsInsideTree() || button.IsQueuedForDeletion()) return;
        var labels = button.GetParent().GetChildren().OfType<NEventOptionButton>()
            .Where(option => option.Event is TravelerEvent && !option.IsQueuedForDeletion())
            .Select(option => option.GetNode<MegaRichTextLabel>("%Text")).Where(label => label.HasMeta(MaxSizeKey)).ToArray();
        // Measure with native limits before sharing the smallest size that fits every option.
        foreach (var label in labels) { label.MaxFontSize = label.GetMeta(MaxSizeKey).AsInt32(); label.Call("AdjustFontSize"); }
        var size = labels.Min(label => label.GetThemeFontSize("normal_font_size"));
        foreach (var label in labels) { label.MaxFontSize = size; label.Call("AdjustFontSize"); }
    }
}
