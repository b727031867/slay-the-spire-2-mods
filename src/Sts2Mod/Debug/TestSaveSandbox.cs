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
        var saves = new SaveManager(new GodotFileIo(Root.Replace('\\', '/')));
        SaveManager.MockInstanceForTesting(saves);
        saves.InitSettingsData();
        saves.SettingsSave.SkipIntroLogo = true;
        GD.Print("STS2MOD_SANDBOX storage=native_disk cloud=false marker=V1");
    }
}
