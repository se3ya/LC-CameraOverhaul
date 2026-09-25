using UnityEngine;

namespace CameraOverhaul;

public static class CameraOverhaulApi
{
    private static float _externalSuppression;

    public static float ExternalSuppression
    {
        get => _externalSuppression;
        set => _externalSuppression = Mathf.Clamp01(value);
    }

    public static void AddTrauma(float amount) => ScreenShakes.AddTrauma(amount);

    public static void BumpTrauma(float level) => ScreenShakes.BumpTrauma(level);

    public static void BumpTrauma(float level, float hold) => ScreenShakes.BumpTrauma(level, hold);

    public static void AddKick(Vector3 strength) => PlayerControllerBPatch.Rig.AddDamageKick(strength);

    public static void SkipVanillaShake() => HUDManagerPatch.SkipThisFrame();

    internal static float EffectScale => 1f - _externalSuppression;
}
