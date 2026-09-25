using HarmonyLib;

namespace CameraOverhaul;

[HarmonyPatch(typeof(VehicleController))]
internal static class VehicleControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("DamagePlayerInVehicle")]
    private static void DamagePlayerInVehiclePrefix() => HUDManagerPatch.BeginSkip();

    [HarmonyPostfix]
    [HarmonyPatch("DamagePlayerInVehicle")]
    private static void DamagePlayerInVehiclePostfix() => HUDManagerPatch.EndSkip();
}
