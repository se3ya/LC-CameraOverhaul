using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(Landmine))]
internal static class LandminePatch
{
    private const float Radius = 20f;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Landmine.SpawnExplosion))]
    private static void SpawnExplosionPostfix(Vector3 explosionPosition)
    {
        var g = ConfigManager.Data.general;
        if (!g.enableScreenShake || g.explosionTrauma <= 0.0) return;

        float factor = ProximityFactor(explosionPosition, Radius);
        if (factor > 0f) ScreenShakes.AddTrauma((float)g.explosionTrauma * factor);
    }

    internal static float ProximityFactor(Vector3 source, float radius)
    {
        PlayerControllerB? lp = StartOfRound.Instance?.localPlayerController;
        if (lp == null) return 0f;
        float dist = Vector3.Distance(lp.gameplayCamera.transform.position, source);
        return 1f - Mathf.Clamp01(dist / radius);
    }
}
