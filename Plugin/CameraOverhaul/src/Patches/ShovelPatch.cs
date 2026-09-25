using HarmonyLib;

namespace CameraOverhaul;

[HarmonyPatch(typeof(Shovel))]
internal static class ShovelPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Shovel.HitShovel))]
    private static void HitShovelPostfix(Shovel __instance)
        => MeleeShake.Apply(__instance.playerHeldBy, __instance.objectsHitByShovelList);
}
