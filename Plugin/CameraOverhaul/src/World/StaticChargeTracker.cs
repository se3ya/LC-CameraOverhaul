using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(StormyWeather))]
internal static class StaticChargeTracker
{
    private const float WarningLength = 10f;
    private const float MinDuration = 0.5f;
    private const float Grace = 1f;
    private const float StoredItemX = 3000f;

    private static GrabbableObject? _item;
    private static float _start;
    private static float _duration;

    public static void Reset() => _item = null;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(StormyWeather.SetStaticElectricityWarning))]
    private static void SetStaticElectricityWarningPostfix(NetworkObject warningObject, float particleTime, bool __runOriginal)
    {
        if (!__runOriginal) return;

        _item = warningObject != null ? warningObject.GetComponent<GrabbableObject>() : null;
        _start = Time.time;
        _duration = Mathf.Max(WarningLength - particleTime, MinDuration);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(StormyWeather.LightningStrike))]
    private static void LightningStrikePostfix(bool useTargetedObject)
    {
        if (useTargetedObject) _item = null;
    }

    public static bool TryGetLocalCharge(PlayerControllerB player, out float charge)
    {
        charge = 0f;
        GrabbableObject? item = _item;
        if (item == null) return false;

        float elapsed = Time.time - _start;
        if (elapsed > _duration + Grace)
        {
            _item = null;
            return false;
        }

        if (item.isInFactory || item.targetFloorPosition.x == StoredItemX || !IsCarrying(player, item)) return false;

        charge = Mathf.Clamp01(elapsed / _duration);
        return true;
    }

    private static bool IsCarrying(PlayerControllerB player, GrabbableObject item)
    {
        if (player.ItemOnlySlot == item) return true;

        GrabbableObject[]? slots = player.ItemSlots;
        if (slots == null) return false;

        for (int i = 0; i < slots.Length; i++)
            if (slots[i] == item) return true;
        return false;
    }
}
