using BaseLib.Audio;
using Godot;
using MegaCrit.Sts2.Core.Saves;
using Sts2Mod.Combo;

namespace Sts2Mod.Presentation;

public static class ElementAudio
{
    private static readonly Dictionary<string, ulong> LastPlayed = [];
    public static string? ElementPath(Element element) => element switch
    {
        Element.Wind => "res://Sts2Mod/audio/wind.wav",
        Element.Rock => "res://Sts2Mod/audio/rock.wav",
        Element.Lightning => "res://Sts2Mod/audio/lightning.wav",
        _ => null
    };
    public const string ComboPath = "res://Sts2Mod/audio/combo.wav";
    internal static bool IsAudible(float master, float sfx) => master > 0 && sfx > 0;

    public static bool Play(string? path)
    {
        if (path is null || DisplayServer.GetName() == "headless") return false;
        var settings = SaveManager.Instance.SettingsSave;
        if (!IsAudible(settings.VolumeMaster, settings.VolumeSfx)) return false;
        var now = Time.GetTicksMsec();
        // One cue per 100ms prevents simultaneous replays from stacking identical sounds.
        if (LastPlayed.TryGetValue(path, out var last) && now - last < 100) return false;
        var player = ModAudio.PlaySoundInRun(path, volumeAdd: -6f);
        if (player is null) return false;
        LastPlayed[path] = now;
        return true;
    }
}
