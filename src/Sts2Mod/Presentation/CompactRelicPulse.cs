using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Sts2Mod.Powers;
using Sts2Mod.Relics;

namespace Sts2Mod.Presentation;

/// <summary>One bounded accent on the existing inventory icon; never copies the relic texture.</summary>
public partial class CompactRelicPulse : Control
{
    public const double Duration = 0.24;
    public const float PeakOpacity = 0.55f;
    private double _elapsed;
    public static bool Handles(RelicModel relic) => relic is JourneyCompass or UnboundJourneyCompass or PaimonCheer;
    public static bool Handles(PowerModel power) => power is ElementResonancePower or FirstElementResonancePower;

    public static CompactRelicPulse Show(TextureRect icon)
    {
        var current = icon.GetChildren().OfType<CompactRelicPulse>().FirstOrDefault(n => !n.IsQueuedForDeletion());
        if (current is not null) { current.Restart(); return current; }
        var pulse = new CompactRelicPulse { Name = "TravelerRelicPulse", MouseFilter = MouseFilterEnum.Ignore, ZIndex = 1 };
        icon.AddChild(pulse);
        pulse.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        pulse.Restart();
        return pulse;
    }

    private void Restart()
    {
        _elapsed = 0;
        Modulate = new Color(1, 1, 1, PeakOpacity);
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        _elapsed += delta;
        if (_elapsed >= Duration) { QueueFree(); return; }
        Modulate = new Color(1, 1, 1, PeakOpacity * (1 - (float)(_elapsed / Duration)));
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (Size.X < 8 || Size.Y < 8) return;
        var accent = new Color("eac27c");
        // All marks remain inside the native icon, with no texture-size-dependent scaling.
        DrawLine(new(Size.X * .25f, Size.Y - 3), new(Size.X * .75f, Size.Y - 3), accent, 2, true);
        DrawArc(Size * .5f, Math.Min(Size.X, Size.Y) * .42f, -1.1f, .3f, 14, accent, 1.5f, true);
    }
}

// Keep RelicModel.Flashed, counters and inventory subscriptions intact.
[HarmonyPatch(typeof(NRelicInventoryHolder), "DoFlash")]
internal static class CompactRelicInventoryFlashPatch
{
    private static bool Prefix(NRelicInventoryHolder __instance)
    {
        if (!CompactRelicPulse.Handles(__instance.Relic.Model)) return true;
        CompactRelicPulse.Show(__instance.Relic.Icon);
        return false;
    }
}

// The two-argument factory calls this factory too. Native callers already accept null in TestMode.
[HarmonyPatch(typeof(NRelicFlashVfx), nameof(NRelicFlashVfx.Create), new[] { typeof(RelicModel) })]
internal static class CompactRelicCreatureFlashPatch
{
    private static bool Prefix(RelicModel relic, ref NRelicFlashVfx? __result)
    {
        if (!CompactRelicPulse.Handles(relic)) return true;
        __result = null;
        return false;
    }
}

// Both resonance variants keep their native Flashed events and energy gain.
[HarmonyPatch(typeof(NPower), "FlashPower")]
internal static class CompactResonanceIconFlashPatch
{
    private static bool Prefix(NPower __instance, TextureRect ____icon)
    {
        if (!CompactRelicPulse.Handles(__instance.Model)) return true;
        CompactRelicPulse.Show(____icon);
        return false;
    }
}

[HarmonyPatch(typeof(NPowerFlashVfx), nameof(NPowerFlashVfx.Create))]
internal static class CompactResonanceCreatureFlashPatch
{
    private static bool Prefix(PowerModel power, ref NPowerFlashVfx? __result)
    {
        if (!CompactRelicPulse.Handles(power)) return true;
        __result = null;
        return false;
    }
}
