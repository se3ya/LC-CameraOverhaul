using UnityEngine;

namespace CameraOverhaul;

internal static class ScreenShakes
{
    private const float TraumaCeiling = 3f;
    private const float MaxShakeAngle = 25f;
    private const NoiseKind ShakeNoise = NoiseKind.FrequencyMod;

    private static float _trauma;
    private static Vector3 _euler;

    public static Vector3 EulerOffset => _euler;

    public static void AddTrauma(float amount)
    {
        if (amount > 0f) _trauma = Mathf.Min(_trauma + amount, TraumaCeiling);
    }

    public static void BumpTrauma(float level)
    {
        if (level > 0f && level > _trauma) _trauma = Mathf.Min(level, TraumaCeiling);
    }

    public static void Reset()
    {
        _trauma = 0f;
        _euler = Vector3.zero;
    }

    public static void OnCameraUpdate(double dt)
    {
        var g = ConfigManager.Data.general;
        _trauma = Mathf.Max(0f, _trauma - (float)(g.screenShakeDecay * dt));

        if (!g.enableScreenShake || _trauma <= 0f)
        {
            _euler = Vector3.zero;
            return;
        }

        float shake = _trauma * _trauma;
        double step = Time.timeAsDouble * g.screenShakesMaxFrequency;
        float scale = shake * (float)g.screenShakesMaxIntensity;
        _euler = new Vector3(
            Mathf.Clamp((float)Noise.Sample(ShakeNoise, step, -69) * scale, -MaxShakeAngle, MaxShakeAngle),
            Mathf.Clamp((float)Noise.Sample(ShakeNoise, step, -420) * scale, -MaxShakeAngle, MaxShakeAngle),
            Mathf.Clamp((float)Noise.Sample(ShakeNoise, step, -1337) * scale, -MaxShakeAngle, MaxShakeAngle));
    }
}