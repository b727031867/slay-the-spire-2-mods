using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace Sts2Mod.Events;

[HarmonyPatch(typeof(NEventOptionButton), nameof(NEventOptionButton._Ready))]
internal static class TravelerEventTypographyPatch
{
    private const string MaxSizeKey = "traveler_option_max_font";
    private sealed record Applied(Node Container, float Width, string Text, int[] Sizes);
    // Resized fires once per option, and measuring changes font sizes again. Every option of
    // one container collapses into a single deferred pass per frame, and a pass is skipped
    // once the container already carries exactly the sizes this patch produced.
    private static readonly Dictionary<ulong, Node> Pending = [];
    private static readonly Dictionary<ulong, Applied> AppliedSizes = [];

    private static void Postfix(NEventOptionButton __instance)
    {
        if (__instance.Event is not TravelerEvent) return;
        var label = __instance.GetNode<MegaRichTextLabel>("%Text");
        label.SetMeta(MaxSizeKey, label.MaxFontSize);
        void Refresh() => Schedule(__instance);
        label.Resized += Refresh;
        Schedule(__instance);
    }

    private static void Schedule(NEventOptionButton button)
    {
        if (!GodotObject.IsInstanceValid(button)) return;
        var container = button.GetParent();
        if (container is null) return;
        var id = container.GetInstanceId();
        if (!Pending.TryAdd(id, container)) return;
        // Closed event screens can free their containers without a flush; prune those entries
        // so visiting many events in one run cannot accumulate cache rows.
        if (AppliedSizes.Count > 8)
            foreach (var stale in AppliedSizes.Where(entry => !GodotObject.IsInstanceValid(entry.Value.Container))
                         .Select(entry => entry.Key).ToArray())
                AppliedSizes.Remove(stale);
        Callable.From(() => Flush(id)).CallDeferred();
    }

    private static void Flush(ulong id)
    {
        if (!Pending.Remove(id, out var container)) return;
        if (!GodotObject.IsInstanceValid(container)) { AppliedSizes.Remove(id); return; }
        Synchronize(id, container);
    }

    private static void Synchronize(ulong id, Node container)
    {
        var options = container.GetChildren().OfType<NEventOptionButton>()
            .Where(option => option.Event is TravelerEvent && GodotObject.IsInstanceValid(option) && !option.IsQueuedForDeletion())
            .ToArray();
        var labels = options.Select(option => option.GetNodeOrNull<MegaRichTextLabel>("%Text"))
            .OfType<MegaRichTextLabel>().Where(label => GodotObject.IsInstanceValid(label) && label.HasMeta(MaxSizeKey))
            .ToArray();
        if (labels.Length == 0) { AppliedSizes.Remove(id); return; }
        var width = (container as Control)?.Size.X ?? 0f;
        var text = string.Join('\u0000', labels.Select(label => label.Text));
        var sizes = labels.Select(label => label.GetThemeFontSize("normal_font_size")).ToArray();
        if (AppliedSizes.TryGetValue(id, out var applied) && ReferenceEquals(applied.Container, container)
            && applied.Width == width && applied.Text == text && applied.Sizes.SequenceEqual(sizes))
            return;
        // Measure with native limits before sharing the smallest size that fits every option.
        foreach (var label in labels) { label.MaxFontSize = label.GetMeta(MaxSizeKey).AsInt32(); label.Call("AdjustFontSize"); }
        var size = labels.Min(label => label.GetThemeFontSize("normal_font_size"));
        foreach (var label in labels) { label.MaxFontSize = size; label.Call("AdjustFontSize"); }
        // Record what the labels actually ended up with, so our own follow-up Resized events
        // are recognized as already applied and cannot ratchet the size down again.
        AppliedSizes[id] = new Applied(container, width, text,
            labels.Select(label => label.GetThemeFontSize("normal_font_size")).ToArray());
    }
}
