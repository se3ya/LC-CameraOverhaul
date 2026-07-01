using System;
using UnityEngine;
using GameNetcodeStuff;

namespace CameraOverhaul;

internal sealed class CameraSystem
{
    private const NoiseKind SwayNoise = NoiseKind.Fractal;
    private const NoiseKind ExhaustionNoise = NoiseKind.Fractal;
    private const NoiseKind InsanityNoise = NoiseKind.Fractal;
    private const NoiseKind DrunknessNoise = NoiseKind.OpenSimplex2;
    private const NoiseKind TinnitusNoise = NoiseKind.Simplex;
    private const NoiseKind CriticalNoise = NoiseKind.Fractal;
    private const NoiseKind PoisonNoise = NoiseKind.Simplex;
    private const NoiseKind JetpackNoise = NoiseKind.Fractal;
    private const NoiseKind ShockNoise = NoiseKind.FrequencyMod;
    private const NoiseKind ShipNoise = NoiseKind.Fractal;

    private Vector3 _offsetEuler;
    private Vector3 _impairOffset;
    private Vector3 _smoothing;
    private float _master = 1f;
    private float _masterBase = 1f;
    private ConfigData.Contextual? _ctxCfg;

    private double _prevPitch;
    private double _prevYaw;
    private double _lastActionTime;

    // effect state
    private double _prevVerticalPitch;
    private double _prevForwardPitch;
    private double _turningRollTarget;
    private double _prevStrafingRoll;
    private double _swayFactor;
    private double _swayFactorTarget;

    // each effect its own noise clock
    private double _swayTime;
    private double _exhaustionTime;
    private double _insanityTime;
    private double _drunkTime;
    private double _criticalTime;
    private double _poisonTime;
    private double _jetpackTime;
    private double _shockTime;
    private double _shipTime;
    private double _exhaustionSeverity;
    private double _criticalSeverity;
    private double _jetpackSeverity;
    private double _shockSeverity;
    private Vector3 _punch;
    private Vector3 _punchVel;
    private bool _msInit;
    private double _prevYawNorm, _prevPitchNorm, _contYaw, _contPitch, _smYaw, _smPitch;

    private static double Now => Time.timeAsDouble;

    public Vector3 OffsetEuler => _offsetEuler * _master + _impairOffset * _masterBase + _smoothing + ScreenShakes.EulerOffset;

    public void Reset()
    {
        _offsetEuler = Vector3.zero;
        _impairOffset = Vector3.zero;
        _smoothing = Vector3.zero;
        _ctxCfg = null;
        _prevPitch = 0;
        _prevYaw = 0;
        _lastActionTime = 0;
        _prevVerticalPitch = 0;
        _prevForwardPitch = 0;
        _turningRollTarget = 0;
        _prevStrafingRoll = 0;
        _swayFactor = 0;
        _swayFactorTarget = 0;
        _swayTime = 0;
        _exhaustionTime = 0;
        _insanityTime = 0;
        _drunkTime = 0;
        _criticalTime = 0;
        _poisonTime = 0;
        _jetpackTime = 0;
        _shockTime = 0;
        _shipTime = 0;
        _exhaustionSeverity = 0;
        _criticalSeverity = 0;
        _jetpackSeverity = 0;
        _shockSeverity = 0;
        _punch = Vector3.zero;
        _punchVel = Vector3.zero;
        _msInit = false;
        ScreenShakes.Reset();
    }

    private const double PunchImpulseScale = 15.0;

    public void AddLandingImpulse(double severity)
        => _punchVel.x += (float)(ConfigManager.Data.general.landingDipStrength * severity * PunchImpulseScale);

    private const double RecoilImpulseScale = 35.0;

    public void AddRecoil(double kickDeg)
    {
        float kick = -(float)(kickDeg * RecoilImpulseScale);
        if (kick < _punchVel.x) _punchVel.x = kick;
    }

    public void AddDamageKick(Vector3 degrees)
        => _punchVel += degrees * (float)PunchImpulseScale;

    public void OnCameraUpdate(in CameraContext context, double dt, PlayerControllerB player)
    {
        ConfigData cfg = ConfigManager.Data;

        float masterMulti = 1f;
        if (cfg.general.enableHealthCondition && player.health < cfg.general.healthConditionTriggerLimit)
        {
            masterMulti = Mathf.Clamp01(player.health / (float)cfg.general.healthConditionTriggerLimit);
        }

        _masterBase = (float)cfg.general.masterStrength;
        _master = _masterBase * masterMulti;
        UpdateContext(in context, dt, cfg);

        _offsetEuler = Vector3.zero;
        _impairOffset = Vector3.zero;
        _smoothing = Vector3.zero;

        ScreenShakes.OnCameraUpdate(dt);

        double yawDelta = Math.Abs(MathUtils.UnwrapStep(context.yaw - _prevYaw));
        double pitchDelta = Math.Abs(context.pitch - _prevPitch);
        if (context.velocity.sqrMagnitude > ACTION_VELOCITY_EPS_SQ
            || yawDelta > ACTION_LOOK_EPS || pitchDelta > ACTION_LOOK_EPS
            || context.isInspectingItem)
            _lastActionTime = Now;

        CameraSmoothingOffset(in context, dt);
        if (cfg.general.enableSway) IdleSwayOffset(in context, dt, cfg);
        ExhaustionOffset(in context, dt, cfg);
        InsanityOffset(in context, dt, cfg);
        TinnitusOffset(dt, cfg);
        DrunknessOffset(in context, dt, cfg);
        CriticalInjuryOffset(in context, dt, cfg);
        PoisonOffset(in context, dt, cfg);
        JetpackTurbulenceOffset(in context, dt, cfg);
        ShockOffset(in context, dt, cfg);
        SinkingTiltOffset(in context, cfg);
        ShipMotionOffset(in context, dt, cfg);
        if (cfg.general.enablePitch)
        {
            VerticalVelocityPitchOffset(in context, dt);
            ForwardVelocityPitchOffset(in context, dt);
        }
        PunchOffset(dt);
        if (cfg.general.enableRoll)
        {
            TurningRollOffset(in context, dt, cfg);
            StrafingRollOffset(in context, dt);
        }

        _prevPitch = context.pitch;
        _prevYaw = context.yaw;
    }

    private void UpdateContext(in CameraContext context, double dt, ConfigData cfg)
    {
        _ctxCfg ??= cfg.walking.Clone();

        ConfigData.Contextual target = context.inVehicle ? cfg.cruiser
            : context.isSprinting ? cfg.sprinting
            : cfg.walking;

        double step = cfg.general.contextTransitionSmoothing > 0
            ? MathUtils.DampStep(cfg.general.contextTransitionSmoothing, dt)
            : 1.0;
        _ctxCfg.Lerp(_ctxCfg, target, step);
    }

    private const double ACTION_VELOCITY_EPS_SQ = 0.0004;
    private const double ACTION_LOOK_EPS = 0.05;

    private const float PunchStiffness = 220f;
    private const float PunchDamping = 22f;
    private const float MaxPunch = 60f;

    private void PunchOffset(double dt)
    {
        float fdt = (float)dt;
        _punchVel += (-PunchStiffness * _punch - PunchDamping * _punchVel) * fdt;
        _punch += _punchVel * fdt;
        _punch = Vector3.ClampMagnitude(_punch, MaxPunch);
        _offsetEuler += _punch;
    }

    private const double BASE_VERTICAL_PITCH_SMOOTHING = 0.00004;
    private const double VERTICAL_PITCH_THRESHOLD = 0.4;

    private static double MaxVelocityPitch => ConfigManager.Data.general.maxVelocityPitch;
    private static double MaxVelocityRoll => ConfigManager.Data.general.maxVelocityRoll;

    private void VerticalVelocityPitchOffset(in CameraContext context, double dt)
    {
        double multiplier = _ctxCfg!.verticalVelocityPitchFactor;
        double smoothing = BASE_VERTICAL_PITCH_SMOOTHING * _ctxCfg.verticalVelocitySmoothingFactor;

        double target = context.isClimbing ? 0.0 : context.velocity.y * multiplier;

        if (Math.Abs(target) < VERTICAL_PITCH_THRESHOLD) target = 0;
        target = MathUtils.Clamp(target, -MaxVelocityPitch, MaxVelocityPitch);
        double current = MathUtils.Damp(_prevVerticalPitch, target, smoothing, dt);

        _offsetEuler.x += (float)current;
        _prevVerticalPitch = current;
    }

    private const double BASE_FORWARD_PITCH_SMOOTHING = 0.008;

    private void ForwardVelocityPitchOffset(in CameraContext context, double dt)
    {
        double multiplier = _ctxCfg!.forwardVelocityPitchFactor;
        double smoothing = BASE_FORWARD_PITCH_SMOOTHING * _ctxCfg.horizontalVelocitySmoothingFactor;

        double target = context.isClimbing ? 0.0 : context.forwardRelVelocity.z * multiplier;
        target = MathUtils.Clamp(target, -MaxVelocityPitch, MaxVelocityPitch);
        double current = MathUtils.Damp(_prevForwardPitch, target, smoothing, dt);

        _offsetEuler.x += (float)current;
        _prevForwardPitch = current;
    }

    private const double BASE_TURNING_ROLL_ACCUMULATION = 0.0048;
    private const double BASE_TURNING_ROLL_INTENSITY = 1.25;
    private const double BASE_TURNING_ROLL_SMOOTHING = 0.0825;

    private void TurningRollOffset(in CameraContext context, double dt, ConfigData cfg)
    {
        double decaySmoothing = BASE_TURNING_ROLL_SMOOTHING * cfg.general.turningRollSmoothing;
        double intensity = BASE_TURNING_ROLL_INTENSITY * cfg.general.turningRollIntensity;
        double accumulation = BASE_TURNING_ROLL_ACCUMULATION * cfg.general.turningRollAccumulation;

        double yawDelta = MathUtils.UnwrapStep(_prevYaw - context.yaw);
        if (context.resetSmoothing) yawDelta = 0;

        _turningRollTarget = MathUtils.Damp(_turningRollTarget, 0, decaySmoothing, dt);

        _turningRollTarget = MathUtils.Clamp(_turningRollTarget + yawDelta * accumulation, -1.0, 1.0);

        double roll = MathUtils.Clamp01(TurningEasing(Math.Abs(_turningRollTarget))) * intensity * Math.Sign(_turningRollTarget);
        _offsetEuler.z += (float)roll;
    }

    // https://easings.net/#easeInOutCubic
    private static double TurningEasing(double x)
    {
        if (x < 0.5) return 4.0 * x * x * x;
        double inverseVal = -2.0 * x + 2.0;
        return 1.0 - (inverseVal * inverseVal * inverseVal) / 2.0;
    }

    private const double BASE_STRAFING_ROLL_SMOOTHING = 0.008;

    private void StrafingRollOffset(in CameraContext context, double dt)
    {
        double multiplier = _ctxCfg!.strafingRollFactor;
        double smoothing = BASE_STRAFING_ROLL_SMOOTHING * _ctxCfg.horizontalVelocitySmoothingFactor;

        double target = -context.forwardRelVelocity.x * multiplier;
        target = MathUtils.Clamp(target, -MaxVelocityRoll, MaxVelocityRoll);
        double offset = MathUtils.Damp(_prevStrafingRoll, target, smoothing, dt);

        _offsetEuler.z += (float)offset;
        _prevStrafingRoll = offset;
    }

    private const double BASE_SWAY_SPEED = 0.5;

    private void IdleSwayOffset(in CameraContext context, double dt, ConfigData cfg)
    {
        double time = Now;

        double currentSwayIntensity = cfg.general.cameraSwayIntensity;
        double currentSwayFreq = cfg.general.cameraSwayFrequency;

        if ((time - _lastActionTime) < cfg.general.cameraSwayFadeInDelay)
            _swayFactorTarget = 0;
        else if (_swayFactor == _swayFactorTarget)
            _swayFactorTarget = 1;

        double fadeLen = _swayFactorTarget > 0 ? cfg.general.cameraSwayFadeInLength : cfg.general.cameraSwayFadeOutLength;
        double fadeStep = fadeLen > 0.0 ? dt / fadeLen : 1.0;
        _swayFactor = MathUtils.StepTowards(_swayFactor, _swayFactorTarget, fadeStep);

        double swayBase = _swayFactor * _swayFactor * _swayFactor;
        if (swayBase <= 0.0) return;

        _swayTime += dt * currentSwayFreq * BASE_SWAY_SPEED;

        double scaledIntensity = currentSwayIntensity * swayBase;
        _offsetEuler.x += (float)(Noise.Sample(SwayNoise, _swayTime, 420.0) * scaledIntensity);
        _offsetEuler.y += (float)(Noise.Sample(SwayNoise, _swayTime, 1337.0) * scaledIntensity);
    }

    // hevy breathing sway
    private void ExhaustionOffset(in CameraContext context, double dt, ConfigData cfg)
    {
        if (!cfg.general.enableExhaustionEffect) return;

        double targetExhaustion = 0.0;
        if (context.isExhausted)
        {
            targetExhaustion = 1.0;
        }
        else if (cfg.general.exhaustionTriggerStamina > 0.0 && context.sprintMeter < cfg.general.exhaustionTriggerStamina)
        {
            targetExhaustion = 1.0 - (Math.Max(context.sprintMeter, 0.0) / cfg.general.exhaustionTriggerStamina);
        }

        _exhaustionSeverity = MathUtils.Damp(_exhaustionSeverity, targetExhaustion, 0.02, dt);
        if (_exhaustionSeverity <= 0.01) return;

        _exhaustionTime += dt * 0.5;
        double intensity = cfg.general.exhaustionSwayMultiplier * _exhaustionSeverity * 0.5;
        _offsetEuler.x += (float)(Noise.Sample(ExhaustionNoise, _exhaustionTime, 5500.0) * intensity);
        _offsetEuler.y += (float)(Noise.Sample(ExhaustionNoise, _exhaustionTime * 1.1, 6600.0) * intensity * 0.7);
    }

    // erratic sway
    private const double INSANITY_THRESHOLD = 0.7;

    private void InsanityOffset(in CameraContext context, double dt, ConfigData cfg)
    {
        if (!cfg.general.enableInsanityEffect || context.insanity < INSANITY_THRESHOLD) return;

        float panic = (float)MathUtils.Clamp01((context.insanity - INSANITY_THRESHOLD) / (1.0 - INSANITY_THRESHOLD));
        if (panic <= 0f) return;

        _insanityTime += dt * (1.0 + panic * 1.25);
        double intensity = cfg.general.insanitySwayMultiplier * panic * 0.4;
        _offsetEuler.x += (float)(Noise.Sample(InsanityNoise, _insanityTime, 7700.0) * intensity);
        _offsetEuler.y += (float)(Noise.Sample(InsanityNoise, _insanityTime * 1.2, 8800.0) * intensity * 0.6);
    }

    // disorenting sway
    private void TinnitusOffset(double dt, ConfigData cfg)
    {
        if (!cfg.general.enableTinnitusEffect || SoundManager.Instance == null || SoundManager.Instance.earsRingingTimer <= 0f) return;

        double severity = MathUtils.Clamp01(SoundManager.Instance.earsRingingTimer / 5.0);
        double t = Now;
        const double freq = 1.25;

        double intensity = cfg.general.tinnitusSwayMultiplier * severity;
        _offsetEuler.x += (float)(Noise.Sample(TinnitusNoise, t * freq, 800.0) * intensity);
        _offsetEuler.y += (float)(Noise.Sample(TinnitusNoise, t * freq * 0.8, 900.0) * intensity);
        _offsetEuler.z += (float)(Noise.Sample(TinnitusNoise, t * freq * 1.1, 1000.0) * intensity * 0.5);
    }

    // slow floating roll
    private void DrunknessOffset(in CameraContext context, double dt, ConfigData cfg)
    {
        if (!cfg.general.enableDrunknessEffect || context.drunkness <= 0.0) return;

        _drunkTime += dt;
        double intensity = context.drunkness * cfg.general.drunknessSwayMultiplier;
        _offsetEuler.z += (float)(Noise.Sample(DrunknessNoise, _drunkTime * 0.5, 3000.0) * intensity);
        _offsetEuler.x += (float)(Noise.Sample(DrunknessNoise, _drunkTime * 0.4, 4000.0) * intensity * 0.5);
    }

    // heavy, slow, pulsing sway
    private void CriticalInjuryOffset(in CameraContext context, double dt, ConfigData cfg)
    {
        if (!cfg.general.enableCriticalInjuryEffect) return;

        double target = context.criticallyInjured ? 1.0 : 0.0;
        _criticalSeverity = MathUtils.Damp(_criticalSeverity, target, 0.05, dt);
        if (_criticalSeverity <= 0.01) return;

        _criticalTime += dt * 0.35;
        // slow labored-breathing throb so the heavy sway swells and settles
        double pulse = 0.75 + 0.25 * Math.Sin(Now * 2.4);
        double intensity = cfg.general.criticalInjurySwayMultiplier * _criticalSeverity * pulse;
        _impairOffset.x += (float)(Noise.Sample(CriticalNoise, _criticalTime, 9100.0) * intensity);
        _impairOffset.y += (float)(Noise.Sample(CriticalNoise, _criticalTime * 1.1, 9200.0) * intensity * 0.8);
        _impairOffset.z += (float)(Noise.Sample(CriticalNoise, _criticalTime * 0.6, 9300.0) * intensity * 0.4);
    }

    // fast, erratic jitter
    private void PoisonOffset(in CameraContext context, double dt, ConfigData cfg)
    {
        if (!cfg.general.enablePoisonEffect || context.poison <= 0f) return;

        _poisonTime += dt * 1.5;
        double intensity = MathUtils.Clamp01(context.poison) * cfg.general.poisonSwayMultiplier;
        _impairOffset.x += (float)(Noise.Sample(PoisonNoise, _poisonTime, 10100.0) * intensity);
        _impairOffset.y += (float)(Noise.Sample(PoisonNoise, _poisonTime * 1.3, 10200.0) * intensity * 0.8);
        _impairOffset.z += (float)(Noise.Sample(PoisonNoise, _poisonTime * 0.9, 10300.0) * intensity * 0.5);
    }

    // rolling, unstable flight sway.
    private void JetpackTurbulenceOffset(in CameraContext context, double dt, ConfigData cfg)
    {
        if (!cfg.general.enableJetpackTurbulence) return;

        double target = context.isUsingJetpack ? 1.0 : 0.0;
        _jetpackSeverity = MathUtils.Damp(_jetpackSeverity, target, 0.03, dt);
        if (_jetpackSeverity <= 0.01) return;

        _jetpackTime += dt * 2.0;
        double intensity = cfg.general.jetpackTurbulenceIntensity * _jetpackSeverity;
        _impairOffset.x += (float)(Noise.Sample(JetpackNoise, _jetpackTime, 11100.0) * intensity);
        _impairOffset.y += (float)(Noise.Sample(JetpackNoise, _jetpackTime * 1.2, 11200.0) * intensity);
        _impairOffset.z += (float)(Noise.Sample(JetpackNoise, _jetpackTime * 0.8, 11300.0) * intensity * 0.6);
    }

    // harsh electric buzz
    private void ShockOffset(in CameraContext context, double dt, ConfigData cfg)
    {
        if (!cfg.general.enableShockEffect) return;

        double target = context.isBeingShocked ? 1.0 : 0.0;
        _shockSeverity = MathUtils.Damp(_shockSeverity, target, 0.02, dt);
        if (_shockSeverity <= 0.01) return;

        _shockTime += dt * 25.0;
        double intensity = cfg.general.shockShakeMultiplier * _shockSeverity;
        _impairOffset.x += (float)(Noise.Sample(ShockNoise, _shockTime, 12100.0) * intensity);
        _impairOffset.y += (float)(Noise.Sample(ShockNoise, _shockTime * 1.1, 12200.0) * intensity);
        _impairOffset.z += (float)(Noise.Sample(ShockNoise, _shockTime * 0.9, 12300.0) * intensity * 0.7);
    }

    // pitch the view down as the player sinks
    private void SinkingTiltOffset(in CameraContext context, ConfigData cfg)
    {
        if (!cfg.general.enableSinkingTilt || context.sinkingValue <= 0f) return;

        double tilt = MathUtils.Clamp01(context.sinkingValue) * cfg.general.sinkingTiltStrength;
        _impairOffset.x += (float)tilt; // +x pitches the view down
    }

    private const double SHIP_SHAKE_DEG = 0.85;     // max degrees of rumble at full strength
    private const double SHIP_RUMBLE_SPEED = 9.0;   // rumble noise clock rate
    private const double SHIP_FORWARD_LURCH = 1.2;  // forward pitch nudge
    private const double SHIP_RISE_PITCH = 0.8;     // takeoff gforce pitch that builds

    private void ShipMotionOffset(in CameraContext context, double dt, ConfigData cfg)
    {
        bool takingOff = context.shipTakeoffPhase >= 0f && cfg.general.shipTakeoffShakeStrength > 0.0;
        bool landing = context.shipLandingPhase >= 0f && cfg.general.shipLandingShakeStrength > 0.0;
        if (!takingOff && !landing) return;

        double phase, strength, env, dirPitch;
        if (takingOff)
        {
            phase = MathUtils.Clamp01(context.shipTakeoffPhase);
            strength = cfg.general.shipTakeoffShakeStrength;
            env = phase * phase; // slow start, ramps up
            double forward = Math.Max(0.0, 1.0 - phase * 3.0);
            dirPitch = (forward * SHIP_FORWARD_LURCH + env * SHIP_RISE_PITCH) * strength;
        }
        else
        {
            phase = MathUtils.Clamp01(context.shipLandingPhase);
            strength = cfg.general.shipLandingShakeStrength;
            env = Math.Sin(phase * Math.PI);
            dirPitch = env * SHIP_FORWARD_LURCH * strength;
        }

        double amp = strength * env * SHIP_SHAKE_DEG;
        if (amp <= 0.0 && dirPitch == 0.0) return;

        _shipTime += dt * SHIP_RUMBLE_SPEED;
        _offsetEuler.x += (float)(Noise.Sample(ShipNoise, _shipTime, 13100.0) * amp + dirPitch);
        _offsetEuler.y += (float)(Noise.Sample(ShipNoise, _shipTime * 1.1, 13200.0) * amp * 0.7);
        _offsetEuler.z += (float)(Noise.Sample(ShipNoise, _shipTime * 0.9, 13300.0) * amp * 0.8);
    }

    private const double BASE_MOUSE_SMOOTHING = 16.0;
    private const double MOUSE_SMOOTHING_THRESHOLD = 0.001;

    private void CameraSmoothingOffset(in CameraContext context, double dt)
    {
        double smoothingValue = Math.Max(0.0, _ctxCfg!.cameraSmoothing);
        double yawNow = context.yaw;
        double pitchNow = context.pitch;

        if (!_msInit || context.resetSmoothing)
        {
            _prevYawNorm = yawNow; _prevPitchNorm = pitchNow;
            _contYaw = yawNow; _contPitch = pitchNow;
            _smYaw = yawNow; _smPitch = pitchNow;
            _msInit = true;
            return;
        }

        double stepYaw = MathUtils.UnwrapStep(yawNow - _prevYawNorm);
        double stepPitch = MathUtils.UnwrapStep(pitchNow - _prevPitchNorm);
        _prevYawNorm = yawNow; _prevPitchNorm = pitchNow;
        _contYaw += stepYaw; _contPitch += stepPitch;

        if (smoothingValue <= MOUSE_SMOOTHING_THRESHOLD)
        {
            _smYaw = _contYaw; _smPitch = _contPitch;
            return;
        }

        double k = BASE_MOUSE_SMOOTHING / smoothingValue;
        double step = 1.0 - Math.Exp(-k * Math.Max(0.0, dt));
        _smYaw += (_contYaw - _smYaw) * step;
        _smPitch += (_contPitch - _smPitch) * step;

        _smoothing.y += (float)MathUtils.UnwrapStep(_smYaw - yawNow);
        _smoothing.x += (float)MathUtils.UnwrapStep(_smPitch - pitchNow);
    }
}