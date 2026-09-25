using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(SandWormAI))]
internal static class SandWormAIPatch
{
    private const float EmergeRadius = 40f;

    [HarmonyPrefix]
    [HarmonyPatch("ShakePlayerCameraInProximity", new[] { typeof(Vector3) })]
    private static void ShakePlayerCameraInProximityPrefix()
    {
        if (ConfigManager.Data.general.enableLeviathanEffects) HUDManagerPatch.BeginSkip();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ShakePlayerCameraInProximity", new[] { typeof(Vector3) })]
    private static void ShakePlayerCameraInProximityPostfix(Vector3 pos)
    {
        if (ConfigManager.Data.general.enableLeviathanEffects) HUDManagerPatch.EndSkip();

        var g = ConfigManager.Data.general;
        if (!g.enableLeviathanEffects) return;

        float factor = LandminePatch.ProximityFactor(pos, EmergeRadius);
        if (factor <= 0f) return;

        if (g.leviathanEmergeTrauma > 0.0)
            ScreenShakes.BumpTrauma((float)g.leviathanEmergeTrauma * factor);
        if (g.leviathanEmergeKick > 0.0)
            PlayerControllerBPatch.Rig.AddDamageKick(new Vector3((float)g.leviathanEmergeKick * factor, 0f, 0f));
    }
}
