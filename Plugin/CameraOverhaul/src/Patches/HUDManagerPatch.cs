using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(HUDManager))]
internal static class HUDManagerPatch
{
    private const double SmallTrauma = 0.5;
    private const double BigTrauma = 1.0;
    private const double LongTrauma = 0.34;
    private const double VeryStrongTrauma = 1.41;

    private const float LongHold = 1.15f;
    private const float VeryStrongHold = 1.2f;

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
            && ConfigManager.Data.general.masterStrength > 0.0;

        if (__state && __instance.playerScreenShakeAnimator != null)
            __instance.playerScreenShakeAnimator.SetBool("ShakingConstant", value: false);

        return !__state;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HUDManager.ShakeCamera))]
    private static void ShakeCameraPostfix(ScreenShakeType shakeType, bool __runOriginal, bool __state)
    {
        if (!__runOriginal && !__state) return;
        if (_apiSkipFrame == Time.frameCount) return;
        if (_skipDepth > 0 && _skipFrame == Time.frameCount) return;

        var g = ConfigManager.Data.general;
        if (!g.enableScreenShake || !g.enableVanillaShakeEvents || g.vanillaShakeStrength <= 0.0) return;

        double trauma;
        float hold;
        switch (shakeType)
        {
            case ScreenShakeType.Small: trauma = SmallTrauma; hold = 0f; break;
            case ScreenShakeType.Big: trauma = BigTrauma; hold = 0f; break;
            case ScreenShakeType.Long: trauma = LongTrauma; hold = LongHold; break;
            case ScreenShakeType.VeryStrong: trauma = VeryStrongTrauma; hold = VeryStrongHold; break;
            default: return;
        }

        _firedFrame = Time.frameCount;
        ScreenShakes.BumpTrauma((float)(trauma * g.vanillaShakeStrength), hold);
    }
}
