using System.Collections.Generic;
using GameNetcodeStuff;
using UnityEngine;

namespace CameraOverhaul;

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
        if (g.meleeWeaponRecoilKick > 0.0) PlayerControllerBPatch.Rig.AddRecoil(g.meleeWeaponRecoilKick * multiplier);
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
