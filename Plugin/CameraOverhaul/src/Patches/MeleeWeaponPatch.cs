using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(Shovel))]
internal static class ShovelPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Shovel.HitShovel))]
    private static void HitShovelPostfix(Shovel __instance)
        => MeleeShake.Apply(__instance.playerHeldBy, __instance.objectsHitByShovelList);
}

[HarmonyPatch(typeof(KnifeItem))]
internal static class KnifeItemPatch
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
        if (g.meleeWeaponRecoilKick > 0.0) PlayerControllerBPatch.System.AddRecoil(g.meleeWeaponRecoilKick * multiplier);
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
