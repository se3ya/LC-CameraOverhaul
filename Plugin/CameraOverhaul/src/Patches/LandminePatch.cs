using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(Landmine))]
internal static class LandminePatch
{
    private const float Radius = 20f;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Landmine.SpawnExplosion))]
    private static void SpawnExplosionPrefix() => HUDManagerPatch.BeginSkip();

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Landmine.SpawnExplosion))]
    private static void SpawnExplosionPostfix(Vector3 explosionPosition)
    {
        HUDManagerPatch.EndSkip();

        var g = ConfigManager.Data.general;
        if (!g.enableScreenShake || g.explosionTrauma <= 0.0) return;

        float factor = ProximityFactor(explosionPosition, Radius);
        if (factor <= 0f) return;

        float trauma = (float)g.explosionTrauma * factor;
        if (HUDManagerPatch.FiredThisFrame) ScreenShakes.BumpTrauma(trauma);
        else ScreenShakes.AddTrauma(trauma);
    }

    internal static float ProximityFactor(Vector3 source, float radius)
    {
        PlayerControllerB? lp = StartOfRound.Instance?.localPlayerController;
        if (lp == null) return 0f;
        float dist = Vector3.Distance(lp.gameplayCamera.transform.position, source);
        return 1f - Mathf.Clamp01(dist / radius);
    }
}
