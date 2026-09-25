using HarmonyLib;

namespace CameraOverhaul;

[HarmonyPatch(typeof(ShotgunItem))]
internal static class ShotgunItemPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ShotgunItem.ShootGun))]
    private static void ShootGunPrefix(ShotgunItem __instance, out bool __state)
    {
        __state = __instance.playerHeldBy != null
            && __instance.playerHeldBy == StartOfRound.Instance?.localPlayerController;
        if (__state) HUDManagerPatch.BeginSkip();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ShotgunItem.ShootGun))]
    private static void ShootGunPostfix(bool __state)
    {
        if (!__state) return;
        HUDManagerPatch.EndSkip();

        var g = ConfigManager.Data.general;
        if (!g.enableWeaponShake) return;

        if (g.weaponShakeTrauma > 0.0) ScreenShakes.BumpTrauma((float)g.weaponShakeTrauma);
        if (g.weaponRecoilKick > 0.0) PlayerControllerBPatch.Rig.AddRecoil(g.weaponRecoilKick);
    }
}
