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
}