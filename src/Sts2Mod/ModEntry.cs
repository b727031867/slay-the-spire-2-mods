using System.Reflection;
using System.Threading;
using BaseLib;
using Godot;
using MegaCrit.Sts2.Core.Modding;

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
        if (baseLibVersion is null || baseLibVersion.Major != 3 || baseLibVersion.Minor != 3 || baseLibVersion.Build != 2)
            throw new InvalidOperationException("Sts2Mod requires the validated BaseLib version 3.3.2.");

        var resource = Godot.FileAccess.GetFileAsString("res://Sts2Mod/smoke.txt").Trim();
        if (resource != "STS2MOD_RESOURCE_OK")
            throw new InvalidOperationException("Sts2Mod resource pack could not be verified.");

        var version = typeof(ModEntry).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        GD.Print($"STS2MOD_READY version={version} baselib=3.3.2 resource=ok");
    }
}
