using System.IO;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Saves;

namespace Sts2Mod.Debug;

/// <summary>Explicit test launches only: real native disk I/O, with no cloud backend.</summary>
internal static class TestSaveSandbox
{
    internal static string Root { get; private set; } = "";

    internal static void Install()
    {
        var path = CommandLineHelper.GetValue("sts2mod-sandbox")
            ?? throw new InvalidOperationException("Automated tests require a dedicated save sandbox.");
        Root = Path.GetFullPath(path);
        var marker = Path.Combine(Root, ".sts2mod-test-sandbox");
        if (!Path.IsPathFullyQualified(path) || !System.IO.File.Exists(marker)
            || System.IO.File.ReadAllText(marker).Trim() != "STS2MOD_TEST_SANDBOX_V1")
            throw new InvalidOperationException("Refusing an unmarked save sandbox.");
        // Installed during mod initialization, before GameStartup starts cloud/profile sync.
        // This is a real SaveManager/GodotFileIo pair, not an in-memory save mock.
        var id = Path.GetFileName(Root);
        if (!Guid.TryParseExact(id, "N", out _)) throw new InvalidOperationException("Invalid test sandbox identity.");
        // GodotFileIo accepts user:// paths, not Windows absolute paths. A unique
        // namespace is separate from steam/<account>/modded/profile*, including settings.
        var nativeRoot = "user://sts2mod-acceptance/" + id;
        if (!CommandLineHelper.HasArg("sts2mod-resume") && DirAccess.DirExistsAbsolute(nativeRoot))
            throw new InvalidOperationException("Preserving an existing native test sandbox.");
        var launchSettings = SaveManager.Instance.SettingsSave;
        var saves = new SaveManager(new GodotFileIo(nativeRoot));
        SaveManager.MockInstanceForTesting(saves);
        // Mod initialization is inside the native settings load. Initialize test
        // display/audio defaults without recursively loading that settings schema.
        // Run, progress and prefs still use the real disk managers on restart.
        saves.InitSettingsDataForTest();
        saves.SettingsSave.ModSettings = launchSettings.ModSettings;
        saves.SettingsSave.Language = launchSettings.Language;
        saves.SettingsSave.VolumeMaster = 0;
        saves.SettingsSave.SkipIntroLogo = true;
        saves.SaveSettings();
        GD.Print("STS2MOD_SANDBOX storage=native_disk cloud=false marker=V1");
    }
}
