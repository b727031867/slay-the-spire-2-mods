using System.Reflection;
using System.Threading;
using BaseLib;
using Godot;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using Sts2Mod.Combo;
using MegaCrit.Sts2.Core.Helpers;
using Sts2Mod.Debug;

namespace Sts2Mod;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
    private static int _initialized;

    public static void Initialize()
    {
        if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0)
            return;

        var baseLibVersion = typeof(BaseLibMain).Assembly.GetName().Version;
        if (baseLibVersion is null || baseLibVersion.Major != 3 || baseLibVersion.Minor != 4 || baseLibVersion.Build != 7)
            throw new InvalidOperationException("Sts2Mod requires the validated BaseLib version 3.4.7.");

        var resource = Godot.FileAccess.GetFileAsString("res://Sts2Mod/smoke.txt").Trim();
        if (resource != "STS2MOD_RESOURCE_OK")
            throw new InvalidOperationException("Sts2Mod resource pack could not be verified.");

        CombatCardElements.Initialize();
        new HarmonyLib.Harmony("Sts2Mod.Traveler").PatchAll(typeof(ModEntry).Assembly);
        ModHelper.SubscribeForCombatStateHooks("Sts2Mod.Combo", _ => [ModelDb.Singleton<ComboLifecycle>()]);
        ModHelper.SubscribeForCombatStateHooks("Sts2Mod.Presentation", _ => [ModelDb.Singleton<Presentation.CombatPresentationLifecycle>()]);
        if (CommandLineHelper.HasArg("sts2mod-debug") && CommandLineHelper.HasArg("sts2mod-test"))
        {
            TestSaveSandbox.Install();
            if (CommandLineHelper.HasArg("sts2mod-rewards") || CommandLineHelper.HasArg("sts2mod-e2e"))
                ModHelper.SubscribeForRunStateHooks("Sts2Mod.RewardProbe", _ => [ModelDb.Singleton<RewardProbe>()]);
            ((SceneTree)Engine.GetMainLoop()).Root.CallDeferred(Node.MethodName.AddChild, new TravelerTestRunner());
        }

        var version = typeof(ModEntry).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        GD.Print($"STS2MOD_READY version={version} baselib=3.4.7 resource=ok");
    }
}
