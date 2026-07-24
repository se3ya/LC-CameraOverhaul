using System;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CameraOverhaul;

internal enum LeviathanCue
{
    None,
    Rumble,
    Growl,
    Emerging,
}

[HarmonyPatch(typeof(StartOfRound))]
internal static class LeviathanTracker
{
    private static SandWormAI[] _worms = Array.Empty<SandWormAI>();
    private static float _rescanTimer;
    private const float RescanInterval = 3f;
    private const float AudibleProximityFloor = 0.3f;

    [HarmonyPostfix]
    [HarmonyPatch("SceneManager_OnLoadComplete1")]
    private static void SceneManager_OnLoadComplete1Postfix()
    {
        _worms = Array.Empty<SandWormAI>();
        _rescanTimer = 0f;
    }

    internal static float GetLocalProximity01(PlayerControllerB player, out LeviathanCue cue)
    {
        cue = LeviathanCue.None;
        var g = ConfigManager.Data.general;
        if (!g.enableLeviathanEffects || g.leviathanProximityStrength <= 0.0 || g.leviathanProximityRadius <= 0.0)
            return 0f;

        _rescanTimer -= Time.deltaTime;
        if (_rescanTimer <= 0f)
        {
            _rescanTimer = RescanInterval;
            _worms = UnityEngine.Object.FindObjectsOfType<SandWormAI>();
        }

        float radius = (float)g.leviathanProximityRadius;
        Camera? cam = player.gameplayCamera;
        Vector3 pos = cam != null ? cam.transform.position : player.transform.position;

        SandWormAI? nearest = null;
        float nearestDist = float.MaxValue;
        for (int i = 0; i < _worms.Length; i++)
        {
            SandWormAI worm = _worms[i];
            if (worm == null || worm.isEnemyDead || worm.emerged) continue;

            Vector3 delta = worm.transform.position - pos;
            delta.y = 0f;
            float dist = delta.magnitude;
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = worm;
            }
        }

        if (nearest == null) return 0f;

        AudioSource sfx = nearest.creatureSFX;
        bool audible = sfx != null && sfx.isPlaying && nearestDist < Mathf.Max(sfx.maxDistance, radius);
        if (!audible) return 0f;

        cue = nearest.inEmergingState ? LeviathanCue.Emerging : ClassifyClip(sfx!.clip);
        return Mathf.Max(1f - Mathf.Clamp01(nearestDist / radius), AudibleProximityFloor);
    }

    private static AudioClip? _classifiedClip;
    private static LeviathanCue _classifiedCue;

    private static LeviathanCue ClassifyClip(AudioClip? clip)
    {
        if (clip == null) return LeviathanCue.None;
        if (clip != _classifiedClip)
        {
            _classifiedClip = clip;
            string name = clip.name;
            _classifiedCue = name.IndexOf("Growl", StringComparison.OrdinalIgnoreCase) >= 0 ? LeviathanCue.Growl
                : name.IndexOf("Rumble", StringComparison.OrdinalIgnoreCase) >= 0 ? LeviathanCue.Rumble
                : LeviathanCue.None;
        }
        return _classifiedCue;
    }
}
