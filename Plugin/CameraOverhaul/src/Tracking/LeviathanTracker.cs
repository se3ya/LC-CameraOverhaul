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

        float best = 0f;
        SandWormAI? nearest = null;
        for (int i = 0; i < _worms.Length; i++)
        {
            SandWormAI worm = _worms[i];
            if (worm == null || worm.isEnemyDead || worm.emerged) continue;

            float dist = Vector3.Distance(pos, worm.transform.position);
            float factor = 1f - Mathf.Clamp01(dist / radius);
            if (factor > best)
            {
                best = factor;
                nearest = worm;
            }
        }

        if (nearest != null)
        {
            AudioSource sfx = nearest.creatureSFX;
            cue = nearest.inEmergingState ? LeviathanCue.Emerging
                : sfx != null && sfx.isPlaying ? ClassifyClip(sfx.clip)
                : LeviathanCue.None;
        }

        return best;
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
