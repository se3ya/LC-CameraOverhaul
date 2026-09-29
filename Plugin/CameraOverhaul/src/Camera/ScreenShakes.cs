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

    private const float ProfileRise = 1f / 3f;

    private static ShakeProfile _profile;
    private static float _profileScale;
    private static float _profileTime = -1f;

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

    public static void Play(in ShakeProfile profile, float scale)
    {
        if (scale <= 0f || profile.Amount <= 0f || profile.Length <= 0f) return;
        _profile = profile;
        _profileScale = scale;
        _profileTime = 0f;
    }

    public static void Reset()
    {
        _profileTime = -1f;
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

        Vector3 profile = g.enableScreenShake ? ProfileOffset(dt, master) : Vector3.zero;

        if (!g.enableScreenShake || _trauma <= 0f)
        {
            _euler = profile;
            return;
        }

        float shake = _trauma * _trauma;
        _shakeTime += dt * g.screenShakesMaxFrequency;
        float scale = shake * (float)g.screenShakesMaxIntensity * master;
        _euler = new Vector3(
            SoftLimit((float)Noise.Sample(ShakeNoise, _shakeTime, -69) * scale),
            SoftLimit((float)Noise.Sample(ShakeNoise, _shakeTime, -420) * scale),
            SoftLimit((float)Noise.Sample(ShakeNoise, _shakeTime, -1337) * scale)) + profile;
    }

    private static Vector3 ProfileOffset(double dt, float master)
    {
        if (_profileTime < 0f) return Vector3.zero;

        _profileTime += (float)dt;
        if (_profileTime >= _profile.Length)
        {
            _profileTime = -1f;
            return Vector3.zero;
        }

        float top = _profile.PeakAt * _profile.Length;
        float tri = _profileTime < top
            ? (top > 0f ? _profileTime / top : 1f)
            : (_profile.Length - _profileTime) / (_profile.Length - top);
        float env = _profile.Floor + ((1f - _profile.Floor) * Mathf.Clamp01(tri));

        float phase = _profileTime * _profile.Rate;
        int index = (int)phase;
        float u = phase - index;
        float shape = u < ProfileRise
            ? Smooth(u / ProfileRise)
            : Smooth(1f - ((u - ProfileRise) / (1f - ProfileRise)));

        float wobble = (float)Noise.Sample(ShakeNoise, index * 0.61, 271);
        float amount = _profile.Amount * env * shape * master * _profileScale
            * (1f - (_profile.Vary * 0.5f * (1f - wobble)));
        if (_profile.Flip && (index & 1) != 0) amount = -amount;

        float jitter = (float)Noise.Sample(ShakeNoise, index * 0.37, -913);
        float angle = (_profile.Angle + (_profile.Spread * jitter)) * Mathf.Deg2Rad;
        return new Vector3(amount * Mathf.Sin(angle), -amount * Mathf.Cos(angle), 0f);
    }

    private static float Smooth(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - (2f * t));
    }

    private static float SoftLimit(float angle)
        => MaxShakeAngle * (float)System.Math.Tanh(angle / MaxShakeAngle);
}
