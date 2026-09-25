using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(DepositItemsDesk))]
internal static class DepositItemsDeskPatch
{
    private const float ShakeRange = 30f;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DepositItemsDesk.MakeLoudNoise))]
    private static void MakeLoudNoisePrefix(DepositItemsDesk __instance, out bool __state)
    {
        __state = !IsNearDesk(__instance);
        if (__state) HUDManagerPatch.BeginSkip();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(DepositItemsDesk.MakeLoudNoise))]
    private static void MakeLoudNoisePostfix(bool __state)
    {
        if (__state) HUDManagerPatch.EndSkip();
    }

    private static bool IsNearDesk(DepositItemsDesk desk)
    {
        PlayerControllerB? lp = StartOfRound.Instance?.localPlayerController;
        AudioSource? audio = desk.deskAudio;
        if (lp == null || audio == null) return false;
        return Vector3.Distance(lp.transform.position, audio.transform.position) < ShakeRange;
    }
}
