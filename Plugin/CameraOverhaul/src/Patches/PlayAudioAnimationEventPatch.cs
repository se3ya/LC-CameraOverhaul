using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

[HarmonyPatch(typeof(PlayAudioAnimationEvent))]
internal static class PlayAudioAnimationEventPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayAudioAnimationEvent.PlayAudio2RandomClip))]
    private static void PlayAudio2RandomClipPostfix(PlayAudioAnimationEvent __instance)
    {
        var g = ConfigManager.Data.general;
        if (!g.enableScreenShake || !g.enableJesterShake || g.jesterStompTrauma <= 0.0) return;

        JesterAI? jester = __instance.GetComponentInParent<JesterAI>();
        if (jester == null || jester.isEnemyDead || jester.currentBehaviourStateIndex != 2) return;

        float proximity = LandminePatch.ProximityFactor(jester.transform.position, (float)g.jesterStompRadius);
        if (proximity <= 0f) return;

        float factor = Mathf.Pow(proximity, (float)g.jesterStompFalloff);
        ScreenShakes.BumpTrauma((float)g.jesterStompTrauma * factor);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayAudioAnimationEvent.PlayAudio1RandomClip))]
    private static void PlayAudio1RandomClipPostfix(PlayAudioAnimationEvent __instance)
    {
        var g = ConfigManager.Data.general;
        if (!g.enableScreenShake || !g.enableForestGiantEffect || g.forestGiantStompTrauma <= 0.0) return;

        ForestGiantAI? giant = __instance.GetComponentInParent<ForestGiantAI>();
        if (giant == null || giant.isEnemyDead) return;

        AudioSource? src = __instance.audioToPlay;
        if (src == null || src.maxDistance <= 0f) return;

        var lp = StartOfRound.Instance?.localPlayerController;
        if (lp == null || lp.gameplayCamera == null) return;

        float minRange = src.minDistance;
        float maxRange = src.maxDistance;
        float dist = Vector3.Distance(lp.gameplayCamera.transform.position, src.transform.position);
        if (dist >= maxRange) return;

        float audible = dist <= minRange ? 1f : 1f - (dist - minRange) / Mathf.Max(0.01f, maxRange - minRange);
        float factor = Mathf.Pow(audible, (float)g.forestGiantStompFalloff);
        ScreenShakes.BumpTrauma((float)g.forestGiantStompTrauma * factor);
    }
}