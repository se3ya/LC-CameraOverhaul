using GameNetcodeStuff;
using UnityEngine;

namespace CameraOverhaul;

internal static class VehicleTracker
{
    private static VehicleController? _active;
    private static float _nextSearchRealtime;

    private const float VehicleSearchInterval = 0.2f;

    public static VehicleController? ResolveActive(PlayerControllerB? localPlayer)
    {
        if (localPlayer == null)
        {
            _active = null;
            return null;
        }

        if (IsPlayerInVehicle(localPlayer, _active))
            return _active;

        float now = Time.realtimeSinceStartup;
        if (now < _nextSearchRealtime)
            return null;

        _nextSearchRealtime = now + VehicleSearchInterval;

        VehicleController[] vehicles = Object.FindObjectsOfType<VehicleController>();
        for (int i = 0; i < vehicles.Length; i++)
        {
            VehicleController vehicle = vehicles[i];
            if (!IsPlayerInVehicle(localPlayer, vehicle))
                continue;

            _active = vehicle;
            return _active;
        }

        _active = null;
        return null;
    }

    public static void ClearCachedActiveIf(PlayerControllerB? localPlayer)
    {
        if (localPlayer == null)
        {
            _active = null;
            return;
        }

        if (!IsPlayerInVehicle(localPlayer, _active))
            _active = null;
    }

    public static void Reset()
    {
        _active = null;
        _nextSearchRealtime = 0f;
    }

    private static bool IsPlayerInVehicle(PlayerControllerB localPlayer, VehicleController? vehicle)
    {
        if (vehicle == null)
            return false;

        return vehicle.currentDriver == localPlayer
            || vehicle.currentPassenger == localPlayer
            || vehicle.localPlayerInControl
            || vehicle.localPlayerInPassengerSeat;
    }
}
