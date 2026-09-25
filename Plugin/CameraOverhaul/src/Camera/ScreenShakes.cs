using UnityEngine;

namespace CameraOverhaul;

internal static class ScreenShakes
{
    private const float TraumaCeiling = 3f;
    private const float MaxShakeAngle = 12f;
    private const NoiseKind ShakeNoise = NoiseKind.FrequencyMod;

    private static float _trauma;
    private static float _sustain;
    private static float _sustainLevel;
    private static double _shakeTime;
    private static Vector3 _euler;

    public static Vector3 EulerOffset => _euler;

    public static void AddTrauma(float amount)
    {
        if (amount > 0f) _trauma = Mathf.Min(_trauma + amount, TraumaCeiling);
    }

    public static void BumpTrauma(float level) => BumpTrauma(level, 0f);

    public static void BumpTrauma(float level, float hold)
    {
        if (level <= 0f) return;

        float capped = Mathf.Min(level, TraumaCeiling);
        if (capped > _trauma) _trauma = capped;

        if (hold <= 0f) return;

        if (capped > _sustainLevel)
        {
            _sustainLevel = capped;
            _sustain = hold;
        }
        else if (capped == _sustainLevel && hold > _sustain)
        {
            _sustain = hold;
        }
    }

    public static void Reset()
    {
        _trauma = 0f;
        _sustain = 0f;
        _sustainLevel = 0f;
        _shakeTime = 0.0;
        _euler = Vector3.zero;
    }

    public static void OnCameraUpdate(double dt, float master)
    {
        var g = ConfigManager.Data.general;
        if (_sustain > 0f)
        {
            _sustain = Mathf.Max(0f, _sustain - (float)dt);
            if (_sustain <= 0f) _sustainLevel = 0f;
        }

        _trauma = Mathf.Max(0f, _trauma - (float)(g.screenShakeDecay * dt));
        if (_trauma < _sustainLevel) _trauma = _sustainLevel;

        if (!g.enableScreenShake || _trauma <= 0f)
        {
            _euler = Vector3.zero;
            return;
        }

        float shake = _trauma * _trauma;
        _shakeTime += dt * g.screenShakesMaxFrequency;
        float scale = shake * (float)g.screenShakesMaxIntensity * master;
        _euler = new Vector3(
            SoftLimit((float)Noise.Sample(ShakeNoise, _shakeTime, -69) * scale),
            SoftLimit((float)Noise.Sample(ShakeNoise, _shakeTime, -420) * scale),
            SoftLimit((float)Noise.Sample(ShakeNoise, _shakeTime, -1337) * scale));
    }

    private static float SoftLimit(float angle)
        => MaxShakeAngle * (float)System.Math.Tanh(angle / MaxShakeAngle);
}
