using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace EnvironmentCheck;

// Compile-time probe only. This project is never automatically installed into the game.
[ModInitializer(nameof(Initialize))]
public static class EnvironmentProbe
{
    public static void Initialize()
    {
        GD.Print($"EnvironmentCheck: {typeof(Harmony).Assembly.GetName().Version}");
    }
}
