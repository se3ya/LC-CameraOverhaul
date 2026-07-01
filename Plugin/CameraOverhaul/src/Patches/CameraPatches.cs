using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(PlayerControllerB))]
internal static class CameraPatches
{
    private static readonly CameraSystem _system = new();
    internal static CameraSystem System => _system;

    private static bool _wasActive;
    private static float _lastYawOffset;
    private static float _lastRollOffset;
    private static Vector3 _lastBodyPos;
    private static bool _hasLastBodyPos;
    private static float _vehicleSpeed;
    private static bool _hasVehicleSpeed;

    private static bool _wasClimbing;
    private static bool _wasInVehicle;
    private static bool _wasInSpecialInteract;
    private static bool _wasInTerminal;

    private const float VehicleImpactDecel = 220f;
    private const float MaxDeltaTime = 0.05f;

    [HarmonyPostfix]
    [HarmonyPatch("LateUpdate")]
    [HarmonyPriority(Priority.Last)]
    private static void LateUpdatePostfix(PlayerControllerB __instance, float ___cameraUp)
    {
        if (__instance != StartOfRound.Instance?.localPlayerController) return;

        Camera? cam = __instance.gameplayCamera;
        CharacterController? controller = __instance.thisController;
        Transform? body = __instance.thisPlayerBody;

        bool teleporting = __instance.teleportedLastFrame;

        if (IsLookLocked(__instance) || teleporting
            || !__instance.isPlayerControlled
            || __instance.isPlayerDead
            || cam == null || controller == null || body == null)
        {
            if (_wasActive && cam != null) RestoreCamera(cam.transform);
            _wasActive = false;
            _hasLastBodyPos = false;
            _hasVehicleSpeed = false;
            ShockTracker.BeingShocked = false;
            return;
        }

        bool justActivated = !_wasActive;
        bool climbingChanged = _wasClimbing != __instance.isClimbingLadder;
        bool vehicleChanged = _wasInVehicle != __instance.inVehicleAnimation;
        bool specialInteractChanged = _wasInSpecialInteract != __instance.inSpecialInteractAnimation;
        bool terminalChanged = _wasInTerminal != __instance.inTerminalMenu;

        bool inControlledCamera = __instance.isClimbingLadder
                               || __instance.inVehicleAnimation
                               || __instance.inSpecialInteractAnimation
                               || __instance.inTerminalMenu;

        bool needsCameraRestore = justActivated || climbingChanged || vehicleChanged
                               || specialInteractChanged || terminalChanged;

        if (justActivated)
            _system.Reset();

        Transform camT = cam.transform;

        if (needsCameraRestore)
        {
            Vector3 prev = camT.localEulerAngles;
            camT.localEulerAngles = new Vector3(prev.x, prev.y - _lastYawOffset, prev.z - _lastRollOffset);
            _lastYawOffset = 0f;
            _lastRollOffset = 0f;
        }

        _wasActive = true;
        _wasClimbing = __instance.isClimbingLadder;
        _wasInVehicle = __instance.inVehicleAnimation;
        _wasInSpecialInteract = __instance.inSpecialInteractAnimation;
        _wasInTerminal = __instance.inTerminalMenu;

        Vector3 cur = camT.localEulerAngles;

        float dt = Mathf.Min(Time.deltaTime, MaxDeltaTime);
        Vector3 vel = ReadVelocity(__instance, body, controller, inControlledCamera, dt) * (float)Constants.VelocityScale;

        Vector3 fwd = body.forward; fwd.y = 0f;
        fwd = fwd.sqrMagnitude > 1e-6f ? fwd.normalized : Vector3.forward;
        Vector3 right = body.right; right.y = 0f;
        right = right.sqrMagnitude > 1e-6f ? right.normalized : Vector3.right;

        ShipMotionTracker.GetLocalShipShakePhases(__instance, out float takeoffPhase, out float landingPhase);

        CameraContext ctx = new CameraContext
        {
            isSprinting = __instance.isSprinting,
            isCrouching = __instance.isCrouching,
            inVehicle = __instance.inVehicleAnimation,
            isClimbing = __instance.isClimbingLadder,
            isExhausted = __instance.isExhausted,
            isInspectingItem = __instance.IsInspectingItem,
            criticallyInjured = __instance.criticallyInjured,
            isUsingJetpack = __instance.jetpackControls,
            isBeingShocked = ShockTracker.BeingShocked,
            sprintMeter = __instance.sprintMeter,
            drunkness = __instance.drunkness,
            insanity = __instance.maxInsanityLevel > 0f
                ? __instance.insanityLevel / __instance.maxInsanityLevel
                : 0f,
            poison = __instance.poison,
            sinkingValue = __instance.sinkingValue,
            shipTakeoffPhase = takeoffPhase,
            shipLandingPhase = landingPhase,
            velocity = vel,
            forwardRelVelocity = new Vector3(Vector3.Dot(vel, right), vel.y, Vector3.Dot(vel, fwd)),
            pitch = ___cameraUp,
            yaw = body.eulerAngles.y,
            resetSmoothing = needsCameraRestore
        };

        _system.OnCameraUpdate(in ctx, dt, __instance);
        Vector3 off = _system.OffsetEuler;

        if (__instance.inVehicleAnimation)
        {
            off.y = 0f;
            off.z = 0f;
        }
        else if (inControlledCamera)
        {
            off = Vector3.zero;
        }

        camT.localEulerAngles = new Vector3(
            cur.x + off.x,
            cur.y - _lastYawOffset + off.y,
            cur.z - _lastRollOffset + off.z);
        _lastYawOffset = off.y;
        _lastRollOffset = off.z;

        VisorCompat.StickVisor(__instance.localVisor, __instance.localVisorTargetPoint, 1.0f);
    }

    private static Vector3 ReadVelocity(PlayerControllerB p, Transform body, CharacterController controller,
        bool inControlledCamera, float dt)
    {
        if (p.inVehicleAnimation)
        {
            VehicleController? vehicle = VehicleTracker.ActiveVehicle;
            if (vehicle == null)
            {
                Vector3 fallback = _hasLastBodyPos ? (body.position - _lastBodyPos) / dt : Vector3.zero;
                _hasVehicleSpeed = false;
                _lastBodyPos = body.position;
                _hasLastBodyPos = true;
                return fallback;
            }

            Vector3 avg = vehicle.averageVelocity;
            float instSpeed = vehicle.mainRigidbody != null ? vehicle.mainRigidbody.velocity.magnitude : avg.magnitude;

            if (_hasVehicleSpeed)
            {
                float decel = (_vehicleSpeed - instSpeed) / dt;
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
            _lastBodyPos = body.position;
            _hasLastBodyPos = true;
            return avg;
        }

        _hasLastBodyPos = false;
        _hasVehicleSpeed = false;
        return inControlledCamera ? Vector3.zero : controller.velocity;
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
        severity *= 1f + (weight - 1f) * (float)g.landingWeightInfluence;

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

    private static bool IsLookLocked(PlayerControllerB p)
        => (p.quickMenuManager != null && p.quickMenuManager.isMenuOpen)
           || p.inSpecialMenu
           || p.disableLookInput
           || (StartOfRound.Instance != null && StartOfRound.Instance.newGameIsLoading);
}
