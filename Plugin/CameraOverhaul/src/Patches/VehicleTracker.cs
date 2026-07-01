using GameNetcodeStuff;
using HarmonyLib;

namespace CameraOverhaul;

[HarmonyPatch(typeof(VehicleController))]
internal static class VehicleTracker
{
    private static VehicleController? _active;

    public static VehicleController? ActiveVehicle => _active;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(VehicleController.Update))]
    private static void UpdatePostfix(VehicleController __instance)
    {
        PlayerControllerB? lp = StartOfRound.Instance?.localPlayerController;
        if (lp == null) return;

        bool inThis = __instance.currentDriver == lp
                   || __instance.currentPassenger == lp
                   || __instance.localPlayerInControl
                   || __instance.localPlayerInPassengerSeat;

        if (inThis)
            _active = __instance;
        else if (_active == __instance)
            _active = null;
    }
}
