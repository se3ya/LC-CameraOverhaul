using HarmonyLib;

namespace CameraOverhaul;

[HarmonyPatch(typeof(JetpackItem))]
internal static class JetpackItemPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("JetpackEffect")]
    private static void JetpackEffectPrefix(JetpackItem __instance, out bool __state)
    {
        __state = __instance.playerHeldBy != null
            && __instance.playerHeldBy == StartOfRound.Instance?.localPlayerController;
        if (__state) HUDManagerPatch.BeginSkip();
    }

    [HarmonyPostfix]
    [HarmonyPatch("JetpackEffect")]
    private static void JetpackEffectPostfix(bool __state)
    {
        if (__state) HUDManagerPatch.EndSkip();
    }
}
