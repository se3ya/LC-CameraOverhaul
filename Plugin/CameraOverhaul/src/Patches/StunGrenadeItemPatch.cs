using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(StunGrenadeItem))]
internal static class StunGrenadeItemPatch
{
    private const float Radius = 30f;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(StunGrenadeItem.StunExplosion))]
    private static void StunExplosionPostfix(Vector3 explosionPosition)
    {
        var g = ConfigManager.Data.general;
        if (!g.enableScreenShake || g.flashbangTrauma <= 0.0) return;

        float factor = LandminePatch.ProximityFactor(explosionPosition, Radius);
        if (factor > 0f) ScreenShakes.AddTrauma((float)g.flashbangTrauma * factor);
    }
}
