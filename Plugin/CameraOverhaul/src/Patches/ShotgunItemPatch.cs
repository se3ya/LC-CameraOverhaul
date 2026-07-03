using HarmonyLib;

namespace CameraOverhaul;

[HarmonyPatch(typeof(ShotgunItem))]
internal static class ShotgunItemPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ShotgunItem.ShootGun))]
    private static void ShootGunPostfix(ShotgunItem __instance)
    {
        if (__instance.playerHeldBy != StartOfRound.Instance?.localPlayerController) return;
        var g = ConfigManager.Data.general;
        if (!g.enableWeaponShake) return;

        if (g.weaponShakeTrauma > 0.0) ScreenShakes.BumpTrauma((float)g.weaponShakeTrauma);
        if (g.weaponRecoilKick > 0.0) PlayerControllerBPatch.System.AddRecoil(g.weaponRecoilKick);
    }
}
