using System;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(PlayerControllerB))]
internal static class PlayerControllerBPatch
{
    private static readonly CameraRig _rig = new();
    internal static CameraRig Rig => _rig;

    private static bool _wasActive;
    private static bool _pendingReset = true;
    private static float _vehicleSpeed;
    private static bool _hasVehicleSpeed;

    private static bool _wasClimbing;
    private static bool _wasInVehicle;
    private static bool _wasInSpecialInteract;
    private static bool _wasInTerminal;
    private static bool _wasInWater;
    private static bool _wasSubmerged;
    private static bool _wasJumping;
    private static Vector3 _prevExternalForce;
    private static bool _wasCameraControlled;
    private static int _damageKickFrame = -1;

    private static float _basePitch;
    private static float _lastPitch;
    private static bool _hasBasePitch;

    private const double VelocityScale = 0.05;
    private const float VehicleImpactDecel = 220f;
    private const float MaxDeltaTime = 0.05f;
    private const float MaxEffectPitch = 85f;
    private const float MaxEffectYaw = 60f;
    private const float MaxEffectRoll = 70f;
    private const float PitchMatchEpsilon = 0.01f;

    [HarmonyPostfix]
    [HarmonyPatch("LateUpdate")]
    [HarmonyPriority(Priority.Last)]
    private static void LateUpdatePostfix(PlayerControllerB __instance, float ___cameraUp)
    {
        if (__instance != StartOfRound.Instance?.localPlayerController) return;

        Camera? cam = __instance.gameplayCamera;
        CharacterController? controller = __instance.thisController;
        Transform? body = __instance.thisPlayerBody;

        if (TryDeactivate(__instance, cam, controller, body)) return;

        bool justActivated = !_wasActive;
        bool inControlledCamera = IsInControlledCamera(__instance);
        bool needsCameraRestore = NeedsCameraRestore(justActivated, __instance);

        if (justActivated && _pendingReset)
        {
            _rig.Reset();
            _pendingReset = false;
        }

        Transform camT = cam.transform;
        MarkCameraStateActive(__instance);

        Vector3 cur = camT.localEulerAngles;
        float dt = Mathf.Min(Time.deltaTime, MaxDeltaTime);

        Vector3 vel = ReadEffectiveVelocity(__instance, controller, inControlledCamera,
            out bool useCruiserEffects, out float effectiveDrunkness);
        TriggerJumpKick(__instance, justActivated);
        TriggerKnockbackKick(__instance, justActivated);
        TriggerPlayerBump(__instance, dt, justActivated);

        float dtScale = Time.deltaTime > 0f ? Mathf.Min(dt / Time.deltaTime, 1f) : 1f;
        CameraContext ctx = BuildContext(__instance, body, camT, ___cameraUp, vel, effectiveDrunkness,
            needsCameraRestore, dtScale);

        _rig.OnCameraUpdate(in ctx, dt, __instance);
        ApplyCameraOffset(__instance, camT, cur, useCruiserEffects, inControlledCamera, ctx.grabbedByEnemy, ___cameraUp);
        ScaleVanillaBob(__instance);
    }

    private static bool TryDeactivate(PlayerControllerB player, Camera? cam, CharacterController? controller, Transform? body)
    {
        if (IsDeactivating(player, cam, controller, body))
        {
            DeactivateCameraState(player, cam, deactivation: true);
            return true;
        }
        if ((IsLookLocked(player) || player.teleportedLastFrame) && !IsGrabbed(player))
        {
            DeactivateCameraState(player, cam, deactivation: false);
            return true;
        }
        return false;
    }

    private static bool IsGrabbed(PlayerControllerB p)
        => p.inAnimationWithEnemy != null && !p.disableLookInput;

    private static Vector3 ReadEffectiveVelocity(PlayerControllerB p, CharacterController controller,
        bool inControlledCamera, out bool useCruiserEffects, out float effectiveDrunkness)
    {
        Vector3 vel = ReadVelocity(p, controller, inControlledCamera, out useCruiserEffects) * (float)VelocityScale;

        effectiveDrunkness = Mathf.Clamp01(p.drunkness);
        if (!ConfigManager.Data.general.enableDrunknessEffect && effectiveDrunkness > 0f)
        {
            vel = RemoveVanillaDrunkSpeedScaling(vel, effectiveDrunkness);
            effectiveDrunkness = 0f;
        }
        return vel;
    }

    private static CameraContext BuildContext(PlayerControllerB p, Transform body, Transform camT,
        float cameraUp, in Vector3 vel, float effectiveDrunkness, bool needsCameraRestore, float dtRatio)
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

        GetColdExposure(p, out bool inSnow, out bool shipWithDoorsOpen, out bool hasActiveLight);

        bool grabbedByBracken = IsBrackenGrab(p, out bool grabbed);
        bool charged = StaticChargeTracker.TryGetLocalCharge(p, out float staticCharge);

        StartOfRound? so = StartOfRound.Instance;

        return new CameraContext
        {
            isSprinting = p.isSprinting,
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
            grabbedByEnemy = grabbed,
            grabbedByBracken = grabbedByBracken,
            isFalling = p.isJumping || p.isFallingFromJump || p.isFallingNoJump,
            gameBobEnabled = IsGameBobEnabled(),
            hasStaticCharge = charged,
            staticCharge = staticCharge,
            carryWeight = p.carryWeight,
            velocity = vel,
            forwardRelVelocity = new Vector3(Vector3.Dot(vel, right), vel.y, Vector3.Dot(vel, fwd)),
            pitch = cameraUp,
            yaw = body.eulerAngles.y,
            dtScale = dtRatio,
            resetSmoothing = needsCameraRestore
        };
    }

    private static void GetColdExposure(PlayerControllerB p, out bool inSnow, out bool shipWithDoorsOpen, out bool hasActiveLight)
    {
        inSnow = false;
        shipWithDoorsOpen = false;
        hasActiveLight = false;

        if (!ConfigManager.Data.general.enableFreezeEffect) return;

        StartOfRound? so = StartOfRound.Instance;
        if (so == null || so.currentLevel == null || !so.currentLevel.levelIncludesSnowFootprints) return;

        inSnow = !p.isInsideFactory && !p.isInHangarShipRoom;
        shipWithDoorsOpen = p.isInHangarShipRoom && !so.hangarDoorsClosed;
        if (!inSnow && !shipWithDoorsOpen) return;

        hasActiveLight = HasActiveLightSource(p) || HasNearbyLightSource(p);
    }

    private static void ApplyCameraOffset(PlayerControllerB p, Transform camT, in Vector3 cur,
        bool useCruiserEffects, bool inControlledCamera, bool grabbed, float cameraUp)
    {
        Vector3 off = _rig.OffsetEuler;

        if (useCruiserEffects)
        {
            off.y = 0f;
            off.z = 0f;
        }
        else if (inControlledCamera && !grabbed)
        {
            off = Vector3.zero;
        }

        float effectScale = CameraOverhaulApi.EffectScale;
        off *= effectScale;

        float basePitch = GetBasePitch(cur.x, p, cameraUp);
        ClampEffectOffset(ref off, basePitch);

        bool freeCamera = grabbed || (!useCruiserEffects && !inControlledCamera);
        float snapYaw = (float)_rig.NeckSnapYaw * effectScale;
        float yaw;
        if (freeCamera) yaw = off.y + snapYaw;
        else if (VanillaOwnsCameraYaw(p) || _wasCameraControlled) yaw = cur.y;
        else yaw = 0f;
        _wasCameraControlled = !freeCamera;
        camT.localEulerAngles = new Vector3(basePitch + off.x, yaw, off.z);

        _basePitch = basePitch;
        _lastPitch = camT.localEulerAngles.x;
        _hasBasePitch = true;

        VisorCompat.StickVisor(p.localVisor, p.localVisorTargetPoint, 1.0f);
    }

    private static bool IsGameBobEnabled()
    {
        if (!ConfigManager.Data.general.followGameBobSetting) return true;
        IngamePlayerSettings? settings = IngamePlayerSettings.Instance;
        return settings == null || settings.settings == null || settings.settings.headBobbing;
    }

    private static void ScaleVanillaBob(PlayerControllerB p)
    {
        float scale = (float)ConfigManager.Data.general.vanillaBobScale;
        scale = Mathf.Lerp(scale, 1f, 1f - CameraOverhaulApi.EffectScale);
        if (scale >= 0.999f) return;

        Transform? container = p.cameraContainerTransform;
        Transform? metarig = p.playerModelArmsMetarig;
        if (container == null || metarig == null || p.inSpecialInteractAnimation) return;

        Vector3 position = container.position;
        position.y = Mathf.Lerp(metarig.position.y, position.y, scale);
        container.position = position;

        if (p.localVisor != null && p.localVisorTargetPoint != null)
            p.localVisor.position = p.localVisorTargetPoint.position;
    }

    private static bool VanillaOwnsCameraYaw(PlayerControllerB p)
        => p.inVehicleAnimation
           || (p.inSpecialInteractAnimation && (p.isClimbingLadder || p.clampLooking));

    private static bool IsBrackenGrab(PlayerControllerB p, out bool grabbed)
    {
        grabbed = IsGrabbed(p);
        return grabbed && p.inAnimationWithEnemy is FlowermanAI;
    }

    private static bool IsDeactivating(PlayerControllerB player, Camera? cam, CharacterController? controller, Transform? body)
        => !player.isPlayerControlled
           || player.isPlayerDead
           || cam == null
           || controller == null
           || body == null
           || (StartOfRound.Instance != null && StartOfRound.Instance.newGameIsLoading);

    private static void DeactivateCameraState(PlayerControllerB player, Camera? cam, bool deactivation)
    {
        if (_wasActive && cam != null)
            RestoreCamera(cam.transform, VanillaOwnsCameraYaw(player));

        _wasActive = false;
        _hasBasePitch = false;
        _wasCameraControlled = false;

        if (!deactivation)
            return;

        _pendingReset = true;
        _hasVehicleSpeed = false;
        _wasInWater = false;
        _wasSubmerged = false;
        StaticChargeTracker.Reset();
        VehicleTracker.Reset();
        ShockTracker.BeingShocked = false;
    }

    private static void TriggerWaterSplashes(bool inWater, bool submerged)
    {
        var g = ConfigManager.Data.general;
        if (g.enableWaterEffect && g.waterSplashStrength > 0.0)
        {
            if (inWater && !_wasInWater) _rig.AddWaterSplash(g.waterSplashStrength * 0.5);
            if (submerged && !_wasSubmerged) _rig.AddWaterSplash(g.waterSplashStrength);
        }
        _wasInWater = inWater;
        _wasSubmerged = submerged;
    }

    private static void TriggerJumpKick(PlayerControllerB p, bool justActivated)
    {
        var g = ConfigManager.Data.general;
        bool jumping = p.isJumping;
        if (jumping && !_wasJumping && !justActivated && g.enableJumpKick && g.jumpKickStrength > 0.0)
            _rig.AddDamageKick(new Vector3((float)g.jumpKickStrength, 0f, 0f));
        _wasJumping = jumping;
    }

    private const float KnockbackMinimum = 1f;
    private const float KnockbackFullScale = 12f;

    private static void TriggerKnockbackKick(PlayerControllerB p, bool justActivated)
    {
        Vector3 force = p.externalForceAutoFade;
        Vector3 delta = force - _prevExternalForce;
        float rise = force.magnitude - _prevExternalForce.magnitude;
        _prevExternalForce = force;

        if (justActivated || _damageKickFrame == Time.frameCount) return;

        var g = ConfigManager.Data.general;
        if (!g.enableKnockbackKick || g.knockbackKickStrength <= 0.0) return;
        if (p.inVehicleAnimation) return;

        float magnitude = delta.magnitude;
        if (rise < KnockbackMinimum || magnitude <= 1e-4f) return;

        Camera? cam = p.gameplayCamera;
        if (cam == null) return;

        float severity = Mathf.Clamp01(rise / KnockbackFullScale);
        Vector3 local = cam.transform.InverseTransformDirection(delta / magnitude);
        float kick = (float)g.knockbackKickStrength * severity;
        _rig.AddDamageKick(new Vector3(local.z * kick, 0f, -local.x * kick));
    }

    private const float PlayerBumpRange = 1.3f;
    private const float PlayerBumpSpeed = 3f;
    private const float PlayerBumpFullScale = 8f;
    private const float PlayerBumpCooldown = 0.5f;
    private const float PlayerBumpTrackRange = 5f;
    private const float PlayerBumpMaxStep = 0.5f;

    private static float[] _bumpGap = new float[0];
    private static float[] _bumpWait = new float[0];
    private static bool _bumpIdle;

    private static void TriggerPlayerBump(PlayerControllerB p, float dt, bool justActivated)
    {
        StartOfRound? so = StartOfRound.Instance;
        if (so == null) return;

        PlayerControllerB[] players = so.allPlayerScripts;
        if (players == null) return;
        if (_bumpGap.Length != players.Length)
        {
            _bumpGap = new float[players.Length];
            _bumpWait = new float[players.Length];
            for (int i = 0; i < _bumpGap.Length; i++) _bumpGap[i] = float.MaxValue;
        }

        var g = ConfigManager.Data.general;
        Camera? cam = p.gameplayCamera;
        bool active = g.enableKnockbackKick && g.playerBumpKick > 0.0
            && !p.inVehicleAnimation && !justActivated && cam != null && dt > 0f;

        if (!active)
        {
            if (_bumpIdle) return;
            for (int i = 0; i < _bumpGap.Length; i++)
            {
                _bumpGap[i] = float.MaxValue;
                _bumpWait[i] = 0f;
            }
            _bumpIdle = true;
            return;
        }

        _bumpIdle = false;
        Vector3 here = p.transform.position;

        for (int i = 0; i < players.Length; i++)
        {
            PlayerControllerB other = players[i];
            if (other == null || other == p || !other.isPlayerControlled || other.isPlayerDead
                || other.isInsideFactory != p.isInsideFactory)
            {
                _bumpGap[i] = float.MaxValue;
                _bumpWait[i] = 0f;
                continue;
            }

            Vector3 away = here - other.transform.position;
            away.y = 0f;
            float sqrGap = away.sqrMagnitude;
            if (sqrGap > PlayerBumpTrackRange * PlayerBumpTrackRange)
            {
                _bumpGap[i] = float.MaxValue;
                _bumpWait[i] = 0f;
                continue;
            }

            float gap = Mathf.Sqrt(sqrGap);
            float last = _bumpGap[i];
            _bumpGap[i] = gap;

            if (_bumpWait[i] > 0f)
            {
                _bumpWait[i] = Mathf.Max(0f, _bumpWait[i] - dt);
                continue;
            }

            if (gap > PlayerBumpRange || gap <= 1e-3f || last > PlayerBumpTrackRange) continue;

            float step = last - gap;
            if (step > PlayerBumpMaxStep || Time.deltaTime > MaxDeltaTime) continue;

            float closing = step / Time.deltaTime;
            if (closing < PlayerBumpSpeed) continue;

            _bumpWait[i] = PlayerBumpCooldown;
            float severity = Mathf.Clamp01(closing / PlayerBumpFullScale);
            Vector3 local = cam!.transform.InverseTransformDirection(away / gap);
            float kick = (float)g.playerBumpKick * severity;
            _rig.AddDamageKick(new Vector3(local.z * kick, 0f, -local.x * kick));
        }
    }

    private const float FearScareMinimum = 0.12f;
    private const float FearRiseFullScale = 0.5f;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerControllerB.JumpToFearLevel))]
    private static void JumpToFearLevelPrefix(out float __state)
        => __state = StartOfRound.Instance != null ? Mathf.Clamp01(StartOfRound.Instance.fearLevel) : 0f;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControllerB.JumpToFearLevel))]
    private static void JumpToFearLevelPostfix(PlayerControllerB __instance, float __state)
    {
        StartOfRound? so = StartOfRound.Instance;
        if (so == null || __instance != so.localPlayerController) return;

        float level = Mathf.Clamp01(so.fearLevel);
        float rise = level - __state;
        if (rise < FearScareMinimum) return;

        _rig.AddScare(level * Mathf.Clamp01(rise / FearRiseFullScale));
    }

    [HarmonyPostfix]
    [HarmonyPatch("PlayFootstepLocal")]
    private static void PlayFootstepLocalPostfix(PlayerControllerB __instance)
    {
        if (__instance != StartOfRound.Instance?.localPlayerController) return;
        if (__instance.isClimbingLadder || __instance.inSpecialInteractAnimation) return;
        _rig.OnFootstep();
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
        bool inControlledCamera, out bool useCruiserEffects)
    {
        float dtSafe = Mathf.Max(Time.deltaTime, 1e-4f);
        useCruiserEffects = false;

        if (p.inVehicleAnimation)
        {
            VehicleController? vehicle = VehicleTracker.GetActive(p);
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
                    _rig.AddDamageKick(new Vector3(3f * sev, 0f, 0f));
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
    private static int _cachedSceneLightCount;
    private static float _sceneLightScanTimer;
    private const float SceneLightScanInterval = 1.0f;

    private static bool HasNearbyLightSource(PlayerControllerB player)
    {
        double radius = ConfigManager.Data.general.freezeLightRadius;
        if (radius <= 0.0) return false;

        _sceneLightScanTimer -= Time.deltaTime;
        if (_sceneLightScanTimer <= 0f)
            RescanSceneLights();

        Vector3 pos = player.transform.position;
        float r2 = (float)(radius * radius);
        for (int i = 0; i < _cachedSceneLightCount; i++)
        {
            Light light = _cachedSceneLights[i];
            if (light == null || !light.isActiveAndEnabled || light.intensity <= 0f)
                continue;
            Vector3 delta = light.transform.position - pos;
            delta.y = 0f;
            if (delta.sqrMagnitude <= r2) return true;
        }

        return false;
    }

    private static void RescanSceneLights()
    {
        _sceneLightScanTimer = SceneLightScanInterval;

        Light[] all = UnityEngine.Object.FindObjectsOfType<Light>();
        int count = 0;
        for (int i = 0; i < all.Length; i++)
        {
            Light light = all[i];
            if (light != null && light.type != LightType.Directional)
                all[count++] = light;
        }

        _cachedSceneLights = all;
        _cachedSceneLightCount = count;
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
        if (!g.enableLandingDip) return;

        _rig.AddLandingImpulse(severity);
        _rig.AddLandingTilt(severity, GetSideSpeed(__instance));
    }

    private static float GetSideSpeed(PlayerControllerB p)
    {
        Transform? body = p.thisPlayerBody;
        if (body == null) return 0f;

        CharacterController? controller = p.thisController;
        if (controller == null) return 0f;

        Vector3 right = body.right;
        right.y = 0f;
        return right.sqrMagnitude > 1e-6f ? Vector3.Dot(controller.velocity, right.normalized) : 0f;
    }

    [HarmonyPrefix]
    [HarmonyPatch("DamagePlayer")]
    private static void DamagePlayerPrefix(PlayerControllerB __instance, out bool __state)
    {
        __state = __instance == StartOfRound.Instance?.localPlayerController
            && __instance.IsOwner && !__instance.isPlayerDead && __instance.AllowPlayerDeath();
        if (__state) HUDManagerPatch.BeginSkip();
    }

    [HarmonyPostfix]
    [HarmonyPatch("DamagePlayer")]
    private static void DamagePlayerPostfix(PlayerControllerB __instance, int damageNumber, Vector3 force, bool __state)
    {
        if (!__state) return;
        HUDManagerPatch.EndSkip();
        if (damageNumber <= 0) return;
        var g = ConfigManager.Data.general;

        float severity = Mathf.Clamp01(damageNumber / 50f);
        if (g.enableScreenShake && g.damageTrauma > 0.0)
            ScreenShakes.BumpTrauma((float)g.damageTrauma * severity);

        Camera? cam = __instance.gameplayCamera;
        Vector3 hitForce = force;
        if (cam == null || g.damageKick <= 0.0 || hitForce.sqrMagnitude <= 1e-4f) return;

        Vector3 local = cam.transform.InverseTransformDirection(hitForce.normalized);
        float kick = (float)g.damageKick * severity;
        _rig.AddDamageKick(new Vector3(local.z * kick, 0f, -local.x * kick));
        _damageKickFrame = Time.frameCount;
    }

    private static void RestoreCamera(Transform camT, bool keepYaw)
    {
        Vector3 cur = camT.localEulerAngles;
        camT.localEulerAngles = new Vector3(GetBasePitch(cur.x, null, 0f), keepYaw ? cur.y : 0f, 0f);
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

    private static bool _aimStripped;
    private static Quaternion _aimRestore;

    [HarmonyPrefix]
    [HarmonyPatch("SetHoverTipAndCurrentInteractTrigger")]
    private static void SetHoverTipPrefix(PlayerControllerB __instance, float ___cameraUp)
    {
        Camera? cam = GetAimCamera(__instance);
        if (cam == null) return;
        float pitch = cam.transform.localEulerAngles.x;
        StripAim(cam, GetBasePitch(pitch, __instance, ___cameraUp));
    }

    [HarmonyFinalizer]
    [HarmonyPatch("SetHoverTipAndCurrentInteractTrigger")]
    private static void SetHoverTipFinalizer(PlayerControllerB __instance) => RestoreAim(__instance);

    [HarmonyPrefix]
    [HarmonyPatch("BeginGrabObject")]
    private static void BeginGrabObjectPrefix(PlayerControllerB __instance)
    {
        Camera? cam = GetAimCamera(__instance);
        if (cam == null || !_hasBasePitch) return;
        StripAim(cam, _basePitch);
    }

    [HarmonyFinalizer]
    [HarmonyPatch("BeginGrabObject")]
    private static void BeginGrabObjectFinalizer(PlayerControllerB __instance) => RestoreAim(__instance);

    private const float AimYawThreshold = 0.25f;

    private static Camera? GetAimCamera(PlayerControllerB p)
    {
        _aimStripped = false;
        if (p != StartOfRound.Instance?.localPlayerController) return null;
        if (VanillaOwnsCameraYaw(p)) return null;

        Camera? cam = p.gameplayCamera;
        if (cam == null) return null;
        return Mathf.Abs(NormalizeSignedAngle(cam.transform.localEulerAngles.y)) >= AimYawThreshold ? cam : null;
    }

    private static void StripAim(Camera cam, float pitch)
    {
        _aimRestore = cam.transform.localRotation;
        _aimStripped = true;
        cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private static void RestoreAim(PlayerControllerB p)
    {
        if (!_aimStripped) return;
        _aimStripped = false;

        Camera? cam = p.gameplayCamera;
        if (cam != null) cam.transform.localRotation = _aimRestore;
    }

    private const float VanillaSmoothLook = 25f;

    private static float GetBasePitch(float currentPitch, PlayerControllerB? player, float cameraUp)
    {
        if (!_hasBasePitch) return currentPitch;

        if (player != null)
        {
            if (player.smoothLookMultiplier != VanillaSmoothLook)
            {
                float step = player.smoothLookMultiplier * Time.deltaTime;
                float expected = Mathf.LerpAngle(_lastPitch, cameraUp, step);
                if (Mathf.Abs(NormalizeSignedAngle(currentPitch - expected)) <= PitchMatchEpsilon)
                    return Mathf.LerpAngle(_basePitch, cameraUp, step);
            }

            if (Mathf.Abs(NormalizeSignedAngle(currentPitch - cameraUp)) <= PitchMatchEpsilon)
                return cameraUp;
        }

        if (Mathf.Abs(NormalizeSignedAngle(currentPitch - _lastPitch)) <= PitchMatchEpsilon)
            return _basePitch;

        return currentPitch;
    }

    private static float NormalizeSignedAngle(float angle)
        => Mathf.Repeat(angle + 180f, 360f) - 180f;

    private static bool IsLookLocked(PlayerControllerB p)
        => (p.quickMenuManager != null && p.quickMenuManager.isMenuOpen)
           || p.inSpecialMenu
           || p.disableLookInput;
}
