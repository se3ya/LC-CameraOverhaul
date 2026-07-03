using System;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(PlayerControllerB))]
internal static class PlayerControllerBPatch
{
    private static readonly CameraSystem _system = new();
    internal static CameraSystem System => _system;

    private static bool _wasActive;
    private static bool _pendingReset = true;
    private static float _lastYawOffset;
    private static float _lastRollOffset;
    private static float _vehicleSpeed;
    private static bool _hasVehicleSpeed;

    private static bool _wasClimbing;
    private static bool _wasInVehicle;
    private static bool _wasInSpecialInteract;
    private static bool _wasInTerminal;
    private static bool _wasInWater;
    private static bool _wasSubmerged;

    private const double VelocityScale = 0.05;
    private const float VehicleImpactDecel = 220f;
    private const float MaxDeltaTime = 0.05f;
    private const float MaxEffectPitch = 85f;
    private const float MaxEffectYaw = 60f;
    private const float MaxEffectRoll = 70f;

    [HarmonyPostfix]
    [HarmonyPatch("LateUpdate")]
    [HarmonyPriority(Priority.Last)]
    private static void LateUpdatePostfix(PlayerControllerB __instance, float ___cameraUp)
    {
        if (__instance != StartOfRound.Instance?.localPlayerController) return;

        Camera? cam = __instance.gameplayCamera;
        CharacterController? controller = __instance.thisController;
        Transform? body = __instance.thisPlayerBody;

        if (HandleInactiveState(__instance, cam, controller, body)) return;

        bool justActivated = !_wasActive;
        bool inControlledCamera = IsInControlledCamera(__instance);
        bool needsCameraRestore = NeedsCameraRestore(justActivated, __instance);

        if (justActivated && _pendingReset)
        {
            _system.Reset();
            _pendingReset = false;
        }

        Transform camT = cam.transform;
        if (needsCameraRestore) RestoreLastOffset(camT);
        MarkCameraStateActive(__instance);

        Vector3 cur = camT.localEulerAngles;
        float dt = Mathf.Min(Time.deltaTime, MaxDeltaTime);

        Vector3 vel = ReadEffectiveVelocity(__instance, controller, inControlledCamera, dt,
            out bool useCruiserEffects, out float effectiveDrunkness);

        CameraContext ctx = BuildContext(__instance, body, camT, ___cameraUp, vel, effectiveDrunkness, needsCameraRestore);

        _system.OnCameraUpdate(in ctx, dt, __instance);
        ApplyCameraOffset(__instance, camT, cur, useCruiserEffects, inControlledCamera);
    }

    private static bool HandleInactiveState(PlayerControllerB player, Camera? cam, CharacterController? controller, Transform? body)
    {
        if (IsDeactivating(player, cam, controller, body))
        {
            DeactivateCameraState(cam, deactivation: true);
            return true;
        }
        if (IsLookLocked(player) || player.teleportedLastFrame)
        {
            DeactivateCameraState(cam, deactivation: false);
            return true;
        }
        return false;
    }

    private static void RestoreLastOffset(Transform camT)
    {
        Vector3 prev = camT.localEulerAngles;
        camT.localEulerAngles = new Vector3(prev.x, prev.y - _lastYawOffset, prev.z - _lastRollOffset);
        _lastYawOffset = 0f;
        _lastRollOffset = 0f;
    }

    private static Vector3 ReadEffectiveVelocity(PlayerControllerB p, CharacterController controller,
        bool inControlledCamera, float dt, out bool useCruiserEffects, out float effectiveDrunkness)
    {
        Vector3 vel = ReadVelocity(p, controller, inControlledCamera, dt, out useCruiserEffects) * (float)VelocityScale;

        effectiveDrunkness = Mathf.Clamp01(p.drunkness);
        if (!ConfigManager.Data.general.enableDrunknessEffect && effectiveDrunkness > 0f)
        {
            vel = RemoveVanillaDrunkSpeedScaling(vel, effectiveDrunkness);
            effectiveDrunkness = 0f;
        }
        return vel;
    }

    private static CameraContext BuildContext(PlayerControllerB p, Transform body, Transform camT,
        float cameraUp, in Vector3 vel, float effectiveDrunkness, bool needsCameraRestore)
    {
        Vector3 fwd = body.forward; fwd.y = 0f;
        fwd = fwd.sqrMagnitude > 1e-6f ? fwd.normalized : Vector3.forward;
        Vector3 right = body.right; right.y = 0f;
        right = right.sqrMagnitude > 1e-6f ? right.normalized : Vector3.right;

        ShipMotionTracker.GetLocalShipShakePhases(p, out float takeoffPhase, out float landingPhase);
        float leviathanProximity = LeviathanTracker.GetLocalProximity01(p, out LeviathanCue leviathanCue);

        bool inWater = p.isUnderwater;
        bool submerged = inWater && p.underwaterCollider != null
                         && p.underwaterCollider.bounds.Contains(camT.position);
        TriggerWaterSplashes(inWater, submerged);

        ComputeColdExposure(p, out bool inSnow, out bool shipWithDoorsOpen, out bool hasActiveLight);

        return new CameraContext
        {
            isSprinting = p.isSprinting,
            isCrouching = p.isCrouching,
            inVehicle = p.inVehicleAnimation,
            isClimbing = p.isClimbingLadder,
            isExhausted = p.isExhausted,
            isInspectingItem = p.IsInspectingItem,
            criticallyInjured = p.criticallyInjured,
            isUsingJetpack = p.jetpackControls,
            isBeingShocked = ShockTracker.BeingShocked,
            sprintMeter = p.sprintMeter,
            drunkness = effectiveDrunkness,
            insanity = p.maxInsanityLevel > 0f
                ? p.insanityLevel / p.maxInsanityLevel
                : 0f,
            poison = p.poison,
            sinkingValue = p.sinkingValue,
            shipTakeoffPhase = takeoffPhase,
            shipLandingPhase = landingPhase,
            inWater = inWater,
            submerged = submerged,
            leviathanProximity01 = leviathanProximity,
            leviathanCue = leviathanCue,
            inSnow = inSnow,
            shipWithDoorsOpen = shipWithDoorsOpen,
            hasActiveLight = hasActiveLight,
            velocity = vel,
            forwardRelVelocity = new Vector3(Vector3.Dot(vel, right), vel.y, Vector3.Dot(vel, fwd)),
            pitch = cameraUp,
            yaw = body.eulerAngles.y,
            resetSmoothing = needsCameraRestore
        };
    }

    private static void ComputeColdExposure(PlayerControllerB p, out bool inSnow, out bool shipWithDoorsOpen, out bool hasActiveLight)
    {
        StartOfRound? so = StartOfRound.Instance;
        bool onSnowyMoon = so != null && so.currentLevel != null && so.currentLevel.levelIncludesSnowFootprints;
        inSnow = onSnowyMoon && !p.isInsideFactory && !p.isInHangarShipRoom;
        shipWithDoorsOpen = onSnowyMoon && p.isInHangarShipRoom && so != null && !so.hangarDoorsClosed;
        hasActiveLight = (inSnow || shipWithDoorsOpen)
            && (HasActiveLightSource(p) || HasNearbyLightSource(p));
    }

    private static void ApplyCameraOffset(PlayerControllerB p, Transform camT, in Vector3 cur,
        bool useCruiserEffects, bool inControlledCamera)
    {
        Vector3 off = _system.OffsetEuler;

        if (useCruiserEffects)
        {
            off.y = 0f;
            off.z = 0f;
        }
        else if (inControlledCamera)
        {
            off = Vector3.zero;
        }

        ClampEffectOffset(ref off, cur.x);

        camT.localEulerAngles = new Vector3(
            cur.x + off.x,
            cur.y - _lastYawOffset + off.y,
            cur.z - _lastRollOffset + off.z);
        _lastYawOffset = off.y;
        _lastRollOffset = off.z;

        VisorCompat.StickVisor(p.localVisor, p.localVisorTargetPoint, 1.0f);
    }

    private static bool IsDeactivating(PlayerControllerB player, Camera? cam, CharacterController? controller, Transform? body)
        => !player.isPlayerControlled
           || player.isPlayerDead
           || cam == null
           || controller == null
           || body == null
           || (StartOfRound.Instance != null && StartOfRound.Instance.newGameIsLoading);

    private static void DeactivateCameraState(Camera? cam, bool deactivation)
    {
        if (_wasActive && cam != null)
            RestoreCamera(cam.transform);

        _wasActive = false;

        if (!deactivation)
            return;

        _pendingReset = true;
        _hasVehicleSpeed = false;
        _wasInWater = false;
        _wasSubmerged = false;
        VehicleTracker.Reset();
        ShockTracker.BeingShocked = false;
    }

    private static void TriggerWaterSplashes(bool inWater, bool submerged)
    {
        var g = ConfigManager.Data.general;
        if (g.enableWaterEffect && g.waterSplashStrength > 0.0)
        {
            if (inWater && !_wasInWater) _system.AddWaterSplash(g.waterSplashStrength * 0.5);
            if (submerged && !_wasSubmerged) _system.AddWaterSplash(g.waterSplashStrength);
        }
        _wasInWater = inWater;
        _wasSubmerged = submerged;
    }

    private static bool IsInControlledCamera(PlayerControllerB player)
        => player.isClimbingLadder
           || player.inVehicleAnimation
           || player.inSpecialInteractAnimation
           || player.inTerminalMenu
           || player.IsInspectingItem;

    private static bool NeedsCameraRestore(bool justActivated, PlayerControllerB player)
        => justActivated
           || _wasClimbing != player.isClimbingLadder
           || _wasInVehicle != player.inVehicleAnimation
           || _wasInSpecialInteract != player.inSpecialInteractAnimation
           || _wasInTerminal != player.inTerminalMenu;

    private static void MarkCameraStateActive(PlayerControllerB player)
    {
        _wasActive = true;
        _wasClimbing = player.isClimbingLadder;
        _wasInVehicle = player.inVehicleAnimation;
        _wasInSpecialInteract = player.inSpecialInteractAnimation;
        _wasInTerminal = player.inTerminalMenu;
    }

    private static Vector3 ReadVelocity(PlayerControllerB p, CharacterController controller,
        bool inControlledCamera, float dt, out bool useCruiserEffects)
    {
        float dtSafe = Mathf.Max(dt, 1e-4f);
        useCruiserEffects = false;

        if (p.inVehicleAnimation)
        {
            VehicleController? vehicle = VehicleTracker.ResolveActive(p);
            bool isVanillaCruiser = vehicle != null
                && vehicle.GetType() == typeof(VehicleController) && vehicle.vehicleID == 0;
            if (vehicle == null || !isVanillaCruiser)
            {
                _hasVehicleSpeed = false;
                return Vector3.zero;
            }

            useCruiserEffects = true;
            Vector3 avg = vehicle.averageVelocity;
            float instSpeed = vehicle.mainRigidbody != null ? vehicle.mainRigidbody.velocity.magnitude : avg.magnitude;

            if (_hasVehicleSpeed)
            {
                float decel = (_vehicleSpeed - instSpeed) / dtSafe;
                if (decel > VehicleImpactDecel && _vehicleSpeed > 4f)
                {
                    var g = ConfigManager.Data.general;
                    float sev = Mathf.Clamp01((decel - VehicleImpactDecel) / 250f);
                    if (g.enableScreenShake && g.vehicleImpactTrauma > 0.0)
                        ScreenShakes.BumpTrauma((float)g.vehicleImpactTrauma * sev);
                    _system.AddDamageKick(new Vector3(3f * sev, 0f, 0f));
                }
            }
            _vehicleSpeed = instSpeed;
            _hasVehicleSpeed = true;
            return avg;
        }

        _hasVehicleSpeed = false;
        VehicleTracker.ClearCachedActiveIf(p);
        return inControlledCamera ? Vector3.zero : controller.velocity;
    }

    private static GrabbableObject? _cachedHeldObject;
    private static Light[] _cachedHeldLights = Array.Empty<Light>();

    private static bool HasActiveLightSource(PlayerControllerB player)
    {
        if (player.helmetLight != null && player.helmetLight.enabled) return true;

        GrabbableObject? held = player.currentlyHeldObjectServer;
        if (held != _cachedHeldObject)
        {
            _cachedHeldObject = held;
            _cachedHeldLights = held != null ? held.GetComponentsInChildren<Light>() : Array.Empty<Light>();
        }

        for (int i = 0; i < _cachedHeldLights.Length; i++)
        {
            Light light = _cachedHeldLights[i];
            if (light != null && light.enabled && light.intensity > 0f) return true;
        }

        return false;
    }

    private static Light[] _cachedSceneLights = Array.Empty<Light>();
    private static float _sceneLightScanTimer;
    private const float SceneLightScanInterval = 1.0f;

    private static bool HasNearbyLightSource(PlayerControllerB player)
    {
        double radius = ConfigManager.Data.general.freezeLightRadius;
        if (radius <= 0.0) return false;

        _sceneLightScanTimer -= Time.deltaTime;
        if (_sceneLightScanTimer <= 0f)
        {
            _sceneLightScanTimer = SceneLightScanInterval;
            _cachedSceneLights = UnityEngine.Object.FindObjectsOfType<Light>();
        }

        Vector3 pos = player.transform.position;
        float r2 = (float)(radius * radius);
        for (int i = 0; i < _cachedSceneLights.Length; i++)
        {
            Light light = _cachedSceneLights[i];
            if (light == null || !light.isActiveAndEnabled || light.type == LightType.Directional || light.intensity <= 0f)
                continue;
            Vector3 delta = light.transform.position - pos;
            delta.y = 0f;
            if (delta.sqrMagnitude <= r2) return true;
        }

        return false;
    }

    private static Vector3 RemoveVanillaDrunkSpeedScaling(Vector3 velocity, float drunkness)
    {
        StartOfRound? so = StartOfRound.Instance;
        if (so == null || so.drunknessSpeedEffect == null)
            return velocity;

        float speedFactor = (so.drunknessSpeedEffect.Evaluate(drunkness) / 5f) + 1f;
        speedFactor = Mathf.Max(speedFactor, 0.25f);
        return velocity / speedFactor;
    }

    [HarmonyPostfix]
    [HarmonyPatch("PlayerHitGroundEffects")]
    private static void PlayerHitGroundEffectsPostfix(PlayerControllerB __instance)
    {
        if (__instance != StartOfRound.Instance?.localPlayerController) return;
        var g = ConfigManager.Data.general;

        float fall = -__instance.fallValue;
        if (fall < 9f) return;
        float severity = Mathf.Clamp01((fall - 9f) / 16f);

        float weight = Mathf.Max(1f, __instance.carryWeight);
        severity *= 1f + ((weight - 1f) * (float)g.landingWeightInfluence);

        if (g.enableScreenShake && g.landingTrauma > 0.0)
            ScreenShakes.BumpTrauma((float)g.landingTrauma * severity);
        if (g.enableLandingDip)
            _system.AddLandingImpulse(severity);
    }

    [HarmonyPostfix]
    [HarmonyPatch("DamagePlayer")]
    private static void DamagePlayerPostfix(PlayerControllerB __instance, int damageNumber, Vector3 force)
    {
        if (__instance != StartOfRound.Instance?.localPlayerController || damageNumber <= 0) return;
        var g = ConfigManager.Data.general;
        if (!g.enableScreenShake) return;

        float severity = Mathf.Clamp01(damageNumber / 50f);
        if (g.damageTrauma > 0.0)
            ScreenShakes.BumpTrauma((float)g.damageTrauma * severity);

        Vector3 hitForce = force;
        if (g.damageKick > 0.0 && hitForce.sqrMagnitude > 1e-4f)
        {
            Vector3 local = __instance.gameplayCamera.transform.InverseTransformDirection(hitForce.normalized);
            float kick = (float)g.damageKick * severity;
            _system.AddDamageKick(new Vector3(local.z * kick, 0f, -local.x * kick));
        }
    }

    private static void RestoreCamera(Transform camT)
    {
        Vector3 cur = camT.localEulerAngles;
        camT.localEulerAngles = new Vector3(cur.x, cur.y - _lastYawOffset, cur.z - _lastRollOffset);
        _lastYawOffset = 0f;
        _lastRollOffset = 0f;
    }

    private static void ClampEffectOffset(ref Vector3 off, float basePitchEuler)
    {
        off.y = Mathf.Clamp(off.y, -MaxEffectYaw, MaxEffectYaw);
        off.z = Mathf.Clamp(off.z, -MaxEffectRoll, MaxEffectRoll);

        float basePitch = NormalizeSignedAngle(basePitchEuler);
        if (basePitch > MaxEffectPitch)
        {
            off.x = Mathf.Min(off.x, 0f);
            return;
        }

        if (basePitch < -MaxEffectPitch)
        {
            off.x = Mathf.Max(off.x, 0f);
            return;
        }

        float minPitchDelta = -MaxEffectPitch - basePitch;
        float maxPitchDelta = MaxEffectPitch - basePitch;
        off.x = Mathf.Clamp(off.x, minPitchDelta, maxPitchDelta);
    }

    private static float NormalizeSignedAngle(float angle)
        => Mathf.Repeat(angle + 180f, 360f) - 180f;

    private static bool IsLookLocked(PlayerControllerB p)
        => (p.quickMenuManager != null && p.quickMenuManager.isMenuOpen)
           || p.inSpecialMenu
           || p.disableLookInput;
}