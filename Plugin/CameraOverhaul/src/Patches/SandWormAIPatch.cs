using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(SandWormAI))]
internal static class SandWormAIPatch
{
    private const float EmergeRadius = 40f;

    [HarmonyPostfix]
    [HarmonyPatch("ShakePlayerCameraInProximity", new[] { typeof(Vector3) })]
    private static void ShakePlayerCameraInProximityPostfix(SandWormAI __instance)
    {
        var g = ConfigManager.Data.general;
        if (!g.enableLeviathanEffects || g.leviathanEmergeTrauma <= 0.0) return;

        float factor = LandminePatch.ProximityFactor(__instance.transform.position, EmergeRadius);
        if (factor <= 0f) return;

        ScreenShakes.BumpTrauma((float)g.leviathanEmergeTrauma * factor);
        PlayerControllerBPatch.System.AddDamageKick(new Vector3((float)g.leviathanEmergeKick * factor, 0f, 0f));
    }
}
