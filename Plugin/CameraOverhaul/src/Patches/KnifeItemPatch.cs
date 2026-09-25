using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(KnifeItem))]
internal static class KnifeItemPatch
{
    private const float SwingInterval = 0.43f;

    [HarmonyPrefix]
    [HarmonyPatch("HitKnife")]
    private static void HitKnifePrefix(float ___timeAtLastDamageDealt, bool cancel, out bool __state)
        => __state = !cancel && Time.realtimeSinceStartup - ___timeAtLastDamageDealt > SwingInterval;

    [HarmonyPostfix]
    [HarmonyPatch("HitKnife")]
    private static void HitKnifePostfix(KnifeItem __instance, List<RaycastHit> ___objectsHitByKnifeList, bool __state)
    {
        if (!__state) return;
        MeleeShake.Apply(__instance.previousPlayerHeldBy, ___objectsHitByKnifeList);
    }
}
