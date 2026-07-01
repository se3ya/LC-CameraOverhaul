using GameNetcodeStuff;
using HarmonyLib;

namespace CameraOverhaul;

// Tracks which vehicle the local player is in. A postfix on VehicleController.Update keeps the cached
// reference current for free (Update runs before the player's LateUpdate), so the camera driver never
// has to scan the scene for it.
[HarmonyPatch(typeof(VehicleController))]
internal static class VehicleTracker
{
    private static VehicleController? _active;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(VehicleController.Update))]
    private static void UpdatePostfix(VehicleController __instance)
    {
        PlayerControllerB? lp = StartOfRound.Instance?.localPlayerController;
        if (IsPlayerInVehicle(lp, __instance))
            _active = __instance;
        else if (_active == __instance)
            _active = null;
    }

    public static VehicleController? ResolveActive(PlayerControllerB? localPlayer)
        => IsPlayerInVehicle(localPlayer, _active) ? _active : null;

    public static void ClearCachedActiveIf(PlayerControllerB? localPlayer)
    {
        if (!IsPlayerInVehicle(localPlayer, _active))
            _active = null;
    }

    public static void Reset() => _active = null;

    private static bool IsPlayerInVehicle(PlayerControllerB? localPlayer, VehicleController? vehicle)
    {
        if (localPlayer == null || vehicle == null)
            return false;

        return vehicle.currentDriver == localPlayer
            || vehicle.currentPassenger == localPlayer
            || vehicle.localPlayerInControl
            || vehicle.localPlayerInPassengerSeat;
    }
}
