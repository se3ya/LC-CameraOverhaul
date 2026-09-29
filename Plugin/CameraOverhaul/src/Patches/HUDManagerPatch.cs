using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(HUDManager))]
internal static class HUDManagerPatch
{
    private static readonly ShakeProfile SmallShake = new(0.58f, 0.217f, 0.15f, 0f, 120f, 8f, 10f, 0f, false);
    private static readonly ShakeProfile BigShake = new(2.0f, 0.6f, 0.17f, 0f, 100f, 30f, 20f, 0.8f, true);
    private static readonly ShakeProfile LongShake = new(0.55f, 1.4f, 0.55f, 0.12f, 124f, 6f, 10f, 0f, false);
    private static readonly ShakeProfile VeryStrongShake = new(1.9f, 1.467f, 0.05f, 0.45f, 115f, 50f, 20f, 0.8f, true);

    private static int _apiSkipFrame = -1;
    private static int _skipFrame = -1;
    private static int _skipDepth;
    private static int _firedFrame = -1;

    internal static void SkipThisFrame() => _apiSkipFrame = Time.frameCount;

    internal static void BeginSkip()
    {
        if (_skipFrame != Time.frameCount) _skipDepth = 0;
        _skipFrame = Time.frameCount;
        _skipDepth++;
    }

    internal static void EndSkip()
    {
        if (_skipDepth > 0) _skipDepth--;
    }

    internal static bool FiredThisFrame => _firedFrame == Time.frameCount;

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(nameof(HUDManager.ShakeCamera))]
    private static bool ShakeCameraPrefix(HUDManager __instance, ScreenShakeType shakeType, bool __runOriginal, out bool __state)
    {
        __state = __runOriginal
            && shakeType != ScreenShakeType.Constant
            && _apiSkipFrame != Time.frameCount
            && CameraOverhaulApi.EffectScale > 0f
            && ConfigManager.Data.general.masterStrength > 0.0
            && !PlayerControllerBPatch.OffsetsMuted;

        if (__state && __instance.playerScreenShakeAnimator != null)
            __instance.playerScreenShakeAnimator.SetBool("ShakingConstant", value: false);

        return !__state;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HUDManager.ShakeCamera))]
    private static void ShakeCameraPostfix(ScreenShakeType shakeType, bool __runOriginal, bool __state)
    {
        if (!__state) return;
        if (_apiSkipFrame == Time.frameCount) return;
        if (_skipDepth > 0 && _skipFrame == Time.frameCount) return;

        var g = ConfigManager.Data.general;
        if (!g.enableScreenShake || !g.enableVanillaShakeEvents || g.vanillaShakeStrength <= 0.0) return;

        ShakeProfile shape;
        switch (shakeType)
        {
            case ScreenShakeType.Small: shape = SmallShake; break;
            case ScreenShakeType.Big: shape = BigShake; break;
            case ScreenShakeType.Long: shape = LongShake; break;
            case ScreenShakeType.VeryStrong: shape = VeryStrongShake; break;
            default: return;
        }

        _firedFrame = Time.frameCount;
        ScreenShakes.Play(in shape, (float)g.vanillaShakeStrength);
    }
}
