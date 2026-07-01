using System;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(StartOfRound))]
internal static class ShipMotionTracker
{
    public static float LandingStartRealtime = -1f;
    private static Collider[] _catwalkColliders = Array.Empty<Collider>();
    private static Collider? _shipBoundsCollider;
    private static Transform? _hangarRoot;
    private static bool _cacheInitialized;
    private static float _nextCatwalkRefreshTime;

    private const float ShipTakeoffDuration = 15f;
    private const float ShipLandingDuration = 10f;
    private const float ShipCacheRefreshInterval = 8f;

    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    private static void AwakePostfix(StartOfRound __instance)
    {
        __instance.StartedLandingShip -= OnStartedLanding;
        __instance.StartedLandingShip += OnStartedLanding;
        LandingStartRealtime = -1f;

        _shipBoundsCollider = null;
        _hangarRoot = null;
        _catwalkColliders = Array.Empty<Collider>();
        _cacheInitialized = false;
        _nextCatwalkRefreshTime = 0f;

        WarmShipReferences(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("SceneManager_OnLoadComplete1")]
    private static void SceneManager_OnLoadComplete1Postfix(StartOfRound __instance)
    {
        WarmShipReferences(__instance);
    }

    private static void OnStartedLanding() => LandingStartRealtime = Time.realtimeSinceStartup;

    internal static void GetLocalShipShakePhases(PlayerControllerB player, out float takeoff, out float landing)
    {
        takeoff = -1f;
        landing = -1f;

        if (!ConfigManager.Data.general.enableScreenShake) return;

        StartOfRound? so = StartOfRound.Instance;
        if (so == null) return;
        if (!IsPlayerOnShipOrCatwalk(player, so)) return;

        float now = Time.realtimeSinceStartup;
        if (so.shipIsLeaving)
        {
            float e = now - so.timeWhenShipStartedLeaving;
            if (e >= 0f && e <= ShipTakeoffDuration) takeoff = e / ShipTakeoffDuration;
        }

        if (LandingStartRealtime >= 0f)
        {
            float e = now - LandingStartRealtime;
            if (e >= 0f && e <= ShipLandingDuration) landing = e / ShipLandingDuration;
            else if (e > ShipLandingDuration) LandingStartRealtime = -1f;
        }
    }

    private static bool IsPlayerOnShipOrCatwalk(PlayerControllerB player, StartOfRound so)
    {
        if (player.isInHangarShipRoom)
            return true;

        EnsureShipReferences(so);

        Vector3 probe = player.transform.position + Vector3.up * 0.25f;
        if (_shipBoundsCollider != null && _shipBoundsCollider.bounds.Contains(probe))
            return true;

        for (int i = 0; i < _catwalkColliders.Length; i++)
        {
            Collider c = _catwalkColliders[i];
            if (c == null) continue;

            Bounds b = c.bounds;
            b.Expand(new Vector3(0.2f, 0.7f, 0.2f));
            if (b.Contains(probe)) return true;
        }

        return false;
    }

    private static void WarmShipReferences(StartOfRound so)
    {
        _nextCatwalkRefreshTime = Time.realtimeSinceStartup + ShipCacheRefreshInterval;
        RebuildShipReferences(so);
    }

    private static void EnsureShipReferences(StartOfRound so)
    {
        float now = Time.realtimeSinceStartup;
        if (_cacheInitialized && now < _nextCatwalkRefreshTime)
            return;

        _nextCatwalkRefreshTime = now + ShipCacheRefreshInterval;
        RebuildShipReferences(so);
    }

    private static void RebuildShipReferences(StartOfRound so)
    {
        _shipBoundsCollider = so.shipBounds;
        _hangarRoot = _shipBoundsCollider != null
            ? _shipBoundsCollider.transform.root
            : FindHangarRoot();

        if (_hangarRoot == null)
        {
            _catwalkColliders = Array.Empty<Collider>();
            _cacheInitialized = true;
            return;
        }

        Collider[] allColliders = _hangarRoot.GetComponentsInChildren<Collider>(includeInactive: true);
        Collider[] found = new Collider[allColliders.Length];
        int count = 0;

        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider c = allColliders[i];
            if (c == null || c.isTrigger) continue;
            if (!HasCatwalkInHierarchy(c.transform, _hangarRoot)) continue;
            found[count++] = c;
        }

        if (count == 0)
        {
            _catwalkColliders = Array.Empty<Collider>();
            _cacheInitialized = true;
            return;
        }

        _catwalkColliders = new Collider[count];
        Array.Copy(found, _catwalkColliders, count);
        _cacheInitialized = true;
    }

    private static Transform? FindHangarRoot()
        => GameObject.Find("HangarShip")?.transform
           ?? GameObject.Find("Environment/HangarShip")?.transform
           ?? GameObject.Find("/Environment/HangarShip")?.transform;

    private static bool HasCatwalkInHierarchy(Transform t, Transform hangarRoot)
    {
        Transform? cur = t;
        while (cur != null)
        {
            if (cur.name.IndexOf("catwalk", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (cur == hangarRoot)
                break;
            cur = cur.parent;
        }

        return false;
    }
}
