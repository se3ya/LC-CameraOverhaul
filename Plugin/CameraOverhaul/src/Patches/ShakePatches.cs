using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(Landmine))]
internal static class ExplosionShakePatch
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

[HarmonyPatch(typeof(StunGrenadeItem))]
internal static class FlashbangShakePatch
{
    private const float Radius = 30f;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(StunGrenadeItem.StunExplosion))]
    private static void StunExplosionPostfix(Vector3 explosionPosition)
    {
        var g = ConfigManager.Data.general;
        if (!g.enableScreenShake || g.flashbangTrauma <= 0.0) return;

        float factor = ExplosionShakePatch.ProximityFactor(explosionPosition, Radius);
        if (factor > 0f) ScreenShakes.AddTrauma((float)g.flashbangTrauma * factor);
    }
}

[HarmonyPatch(typeof(ShotgunItem))]
internal static class ShotgunShakePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ShotgunItem.ShootGun))]
    private static void ShootGunPostfix(ShotgunItem __instance)
    {
        if (__instance.playerHeldBy != StartOfRound.Instance?.localPlayerController) return;
        var g = ConfigManager.Data.general;
        if (!g.enableWeaponShake) return;

        if (g.weaponShakeTrauma > 0.0) ScreenShakes.BumpTrauma((float)g.weaponShakeTrauma);
        if (g.weaponRecoilKick > 0.0) CameraPatches.System.AddRecoil(g.weaponRecoilKick);
    }
}

[HarmonyPatch(typeof(Shovel))]
internal static class ShovelShakePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Shovel.HitShovel))]
    private static void HitShovelPostfix(Shovel __instance)
        => MeleeShake.Apply(__instance.playerHeldBy, __instance.objectsHitByShovelList);
}

[HarmonyPatch(typeof(KnifeItem))]
internal static class KnifeShakePatch
{
    [HarmonyPostfix]
    [HarmonyPatch("HitKnife")]
    private static void HitKnifePostfix(KnifeItem __instance, List<RaycastHit> ___objectsHitByKnifeList)
        => MeleeShake.Apply(__instance.previousPlayerHeldBy, ___objectsHitByKnifeList);
}

internal static class MeleeShake
{
    private const int RoomLayer = 8;
    private const int CollidersLayer = 11;

    public static void Apply(PlayerControllerB? holder, List<RaycastHit>? hits)
    {
        if (holder == null || holder != StartOfRound.Instance?.localPlayerController) return;
        var g = ConfigManager.Data.general;
        if (!g.enableMeleeWeaponShake) return;

        float multiplier = Connected(hits, holder) ? 1f : (float)g.meleeWeaponMissMultiplier;
        if (g.meleeWeaponShakeTrauma > 0.0) ScreenShakes.BumpTrauma((float)g.meleeWeaponShakeTrauma * multiplier);
        if (g.meleeWeaponRecoilKick > 0.0) CameraPatches.System.AddRecoil(g.meleeWeaponRecoilKick * multiplier);
    }

    private static bool Connected(List<RaycastHit>? hits, PlayerControllerB holder)
    {
        if (hits == null) return false;
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            int layer = hit.transform.gameObject.layer;
            if (layer == RoomLayer || layer == CollidersLayer)
            {
                if (!hit.collider.isTrigger) return true;
            }
            else if (hit.transform.TryGetComponent<IHittable>(out _) && hit.transform != holder.transform)
            {
                return true;
            }
        }
        return false;
    }
}