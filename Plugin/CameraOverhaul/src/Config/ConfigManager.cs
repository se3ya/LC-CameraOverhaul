using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using BepInEx.Configuration;

namespace CameraOverhaul;

internal static class ConfigManager
{
    private static readonly ConfigData _data = new();
    public static ConfigData Data => _data;

    private static readonly List<ConfigEntryBase> _entries = new();

    private static ConfigEntry<float> _masterStrength = null!;
    private static ConfigEntry<float> _contextTransitionSmoothing = null!;
    private static ConfigEntry<float> _maxVelocityRoll = null!;
    private static ConfigEntry<float> _maxVelocityPitch = null!;

    private static ConfigEntry<bool> _enableRoll = null!;
    private static ConfigEntry<bool> _enablePitch = null!;
    private static ConfigEntry<bool> _enableSway = null!;
    private static ConfigEntry<bool> _enableScreenShake = null!;
    private static ConfigEntry<bool> _enableLandingDip = null!;
    private static ConfigEntry<bool> _enableWeaponShake = null!;
    private static ConfigEntry<bool> _enableMeleeWeaponShake = null!;
    private static ConfigEntry<bool> _enableTinnitusEffect = null!;
    private static ConfigEntry<bool> _enableExhaustionEffect = null!;
    private static ConfigEntry<bool> _enableInsanityEffect = null!;
    private static ConfigEntry<bool> _enableDrunknessEffect = null!;
    private static ConfigEntry<bool> _enableCriticalInjuryEffect = null!;
    private static ConfigEntry<bool> _enablePoisonEffect = null!;
    private static ConfigEntry<bool> _enableJetpackTurbulence = null!;
    private static ConfigEntry<bool> _enableShockEffect = null!;
    private static ConfigEntry<bool> _enableSinkingTilt = null!;
    private static ConfigEntry<bool> _enableWaterEffect = null!;
    private static ConfigEntry<bool> _enableLeviathanEffects = null!;
    private static ConfigEntry<bool> _enableFreezeEffect = null!;
    private static ConfigEntry<bool> _enableHealthCondition = null!;

    private static ConfigEntry<float> _turningRollIntensity = null!;
    private static ConfigEntry<float> _turningRollAccumulation = null!;
    private static ConfigEntry<float> _turningRollSmoothing = null!;

    private static ConfigEntry<float> _swayIntensity = null!;
    private static ConfigEntry<float> _swayFrequency = null!;
    private static ConfigEntry<float> _swayFadeInDelay = null!;
    private static ConfigEntry<float> _swayFadeInLength = null!;
    private static ConfigEntry<float> _swayFadeOutLength = null!;
    private static ConfigEntry<float> _tinnitusSwayMultiplier = null!;
    private static ConfigEntry<float> _exhaustionSwayMultiplier = null!;
    private static ConfigEntry<float> _exhaustionTriggerStamina = null!;
    private static ConfigEntry<float> _insanitySwayMultiplier = null!;
    private static ConfigEntry<float> _insanityTriggerThreshold = null!;
    private static ConfigEntry<float> _drunknessSwayMultiplier = null!;
    private static ConfigEntry<float> _criticalInjurySwayMultiplier = null!;
    private static ConfigEntry<float> _poisonSwayMultiplier = null!;
    private static ConfigEntry<float> _jetpackTurbulenceIntensity = null!;
    private static ConfigEntry<float> _shockShakeMultiplier = null!;
    private static ConfigEntry<float> _sinkingTiltStrength = null!;
    private static ConfigEntry<float> _waterWadeStrength = null!;
    private static ConfigEntry<float> _waterSubmergedDriftStrength = null!;
    private static ConfigEntry<float> _waterSplashStrength = null!;
    private static ConfigEntry<float> _leviathanEmergeTrauma = null!;
    private static ConfigEntry<float> _leviathanEmergeKick = null!;
    private static ConfigEntry<float> _leviathanProximityStrength = null!;
    private static ConfigEntry<float> _leviathanProximityRadius = null!;
    private static ConfigEntry<float> _leviathanWarningTremorMultiplier = null!;
    private static ConfigEntry<float> _leviathanRumbleShakeMultiplier = null!;
    private static ConfigEntry<float> _leviathanGrowlShakeMultiplier = null!;
    private static ConfigEntry<float> _freezeStrength = null!;
    private static ConfigEntry<float> _freezeBuildSeconds = null!;
    private static ConfigEntry<float> _freezeRecoverSeconds = null!;
    private static ConfigEntry<float> _freezeLightReduce = null!;
    private static ConfigEntry<float> _freezeLightRadius = null!;
    private static ConfigEntry<float> _freezeShipOpenDoorTarget = null!;

    private static ConfigEntry<float> _shakeMaxIntensity = null!;
    private static ConfigEntry<float> _shakeMaxFrequency = null!;
    private static ConfigEntry<float> _shakeDecay = null!;
    private static ConfigEntry<float> _explosionTrauma = null!;
    private static ConfigEntry<float> _landingTrauma = null!;
    private static ConfigEntry<float> _damageTrauma = null!;
    private static ConfigEntry<float> _damageKick = null!;
    private static ConfigEntry<float> _vehicleImpactTrauma = null!;
    private static ConfigEntry<float> _flashbangTrauma = null!;
    private static ConfigEntry<float> _shipTakeoffShakeStrength = null!;
    private static ConfigEntry<float> _shipLandingShakeStrength = null!;
    private static ConfigEntry<float> _weaponShakeTrauma = null!;
    private static ConfigEntry<float> _weaponRecoilKick = null!;
    private static ConfigEntry<float> _meleeWeaponShakeTrauma = null!;
    private static ConfigEntry<float> _meleeWeaponRecoilKick = null!;
    private static ConfigEntry<float> _meleeWeaponMissMultiplier = null!;
    private static ConfigEntry<float> _landingDipStrength = null!;
    private static ConfigEntry<float> _landingWeightInfluence = null!;
    private static ConfigEntry<float> _healthConditionTriggerLimit = null!;

    private sealed class ContextEntries
    {
        public ConfigEntry<float> Strafe = null!;
        public ConfigEntry<float> ForwardPitch = null!;
        public ConfigEntry<float> VerticalPitch = null!;
        public ConfigEntry<float> HSmooth = null!;
        public ConfigEntry<float> VSmooth = null!;
        public ConfigEntry<float> MouseSmoothing = null!;
    }

    private static readonly ContextEntries _walk = new();
    private static readonly ContextEntries _sprint = new();
    private static readonly ContextEntries _cruiser = new();

    internal static void Initialize(ConfigFile config)
    {
        _entries.Clear();

        config.SaveOnConfigSet = false;

        var d = new ConfigData();

        const string general = "1. General";
        _masterStrength = BindFloat(config, general, "MasterStrength", (float)d.general.masterStrength, 0f, 3f,
            "Scales every effect.");
        _contextTransitionSmoothing = BindFloat(config, general, "ContextTransitionSmoothing", (float)d.general.contextTransitionSmoothing, 0f, 1f,
            "How smoothly tuning blends when your movement context changes.");
        _maxVelocityRoll = BindFloat(config, general, "MaxVelocityRoll", (float)d.general.maxVelocityRoll, 0f, 90f,
            "Safety cap on speed-driven roll so very high speeds can't fling the view.");
        _maxVelocityPitch = BindFloat(config, general, "MaxVelocityPitch", (float)d.general.maxVelocityPitch, 0f, 90f,
            "Safety cap on speed-driven pitch so very high speeds can't fling the view.");

        const string toggles = "2. Effect Toggles";
        _enableRoll = BindBool(config, toggles, "EnableRoll", d.general.enableRoll,
            "Strafing lean and turning roll.");
        _enablePitch = BindBool(config, toggles, "EnablePitch", d.general.enablePitch,
            "Forward/back and vertical velocity pitch.");
        _enableSway = BindBool(config, toggles, "EnableSway", d.general.enableSway,
            "Idle camera sway while standing still.");
        _enableScreenShake = BindBool(config, toggles, "EnableScreenShake", d.general.enableScreenShake,
            "Screen shake from explosions, hard landings and taking damage.");
        _enableLandingDip = BindBool(config, toggles, "EnableLandingDip", d.general.enableLandingDip,
            "Quick downward pitch punch when you land hard.");
        _enableWeaponShake = BindBool(config, toggles, "EnableWeaponShake", d.general.enableWeaponShake,
            "Rattle and upward recoil kick when you fire a gun.");
        _enableMeleeWeaponShake = BindBool(config, toggles, "EnableMeleeWeaponShake", d.general.enableMeleeWeaponShake,
            "Rattle and upward punch when you swing a melee weapon.");
        _enableTinnitusEffect = BindBool(config, toggles, "EnableTinnitusEffect", d.general.enableTinnitusEffect,
            "Increases camera sway heavily while having tinnitues.");
        _enableExhaustionEffect = BindBool(config, toggles, "EnableExhaustionEffect", d.general.enableExhaustionEffect,
            "Heavy breathing/sway when you are completely out of stamina.");
        _enableInsanityEffect = BindBool(config, toggles, "EnableInsanityEffect", d.general.enableInsanityEffect,
            "Faster camera sway when character insanity limit drops.");
        _enableDrunknessEffect = BindBool(config, toggles, "EnableDrunknessEffect", d.general.enableDrunknessEffect,
            "Smooth floating camera drift when intoxicated.");
        _enableCriticalInjuryEffect = BindBool(config, toggles, "EnableCriticalInjuryEffect", d.general.enableCriticalInjuryEffect,
            "Pulsing near death sway while critically injured.");
        _enablePoisonEffect = BindBool(config, toggles, "EnablePoisonEffect", d.general.enablePoisonEffect,
            "Jittery sway while poisoned.");
        _enableJetpackTurbulence = BindBool(config, toggles, "EnableJetpackTurbulence", d.general.enableJetpackTurbulence,
            "Turbulence sway while flying a jetpack.");
        _enableShockEffect = BindBool(config, toggles, "EnableShockEffect", d.general.enableShockEffect,
            "Electric jitter while being zapped by a zap gun.");
        _enableSinkingTilt = BindBool(config, toggles, "EnableSinkingTilt", d.general.enableSinkingTilt,
            "Forward pitch tilt as you sink into quicksand.");
        _enableWaterEffect = BindBool(config, toggles, "EnableWaterEffect", d.general.enableWaterEffect,
            "Sloshy sway while wading and floaty drift while submerged in water.");
        _enableLeviathanEffects = BindBool(config, toggles, "EnableLeviathanEffects", d.general.enableLeviathanEffects,
            "Camera shake when an Earth Leviathan starts emerging, emerges back and light tremor while it's close.");
        _enableFreezeEffect = BindBool(config, toggles, "EnableFreezeEffect", d.general.enableFreezeEffect,
            "Freezing that builds up while outside on a snowy moon.");
        _enableHealthCondition = BindBool(config, toggles, "EnableHealthCondition", d.general.enableHealthCondition,
            "When health is low, camera effects gradually fade out.");

        const string turning = "3. Turning Roll";
        _turningRollIntensity = BindFloat(config, turning, "Intensity", (float)d.general.turningRollIntensity, 0f, 5f,
            "Maximum roll the camera leans when you whip the view left/right.");
        _turningRollAccumulation = BindFloat(config, turning, "Accumulation", (float)d.general.turningRollAccumulation, 0f, 5f,
            "How quickly fast turning builds up the lean.");
        _turningRollSmoothing = BindFloat(config, turning, "Smoothing", (float)d.general.turningRollSmoothing, 0f, 5f,
            "How quickly the turning lean settles back to level once you stop turning.");

        const string sway = "4. Camera Sway";
        _swayIntensity = BindFloat(config, sway, "Intensity", (float)d.general.cameraSwayIntensity, 0f, 5f,
            "Strength of the idle sway while standing still.");
        _swayFrequency = BindFloat(config, sway, "Frequency", (float)d.general.cameraSwayFrequency, 0f, 2f,
            "Speed of the idle sway.");
        _swayFadeInDelay = BindFloat(config, sway, "FadeInDelay", (float)d.general.cameraSwayFadeInDelay, 0f, 5f,
            "Seconds of stillness before sway starts fading in.");
        _swayFadeInLength = BindFloat(config, sway, "FadeInLength", (float)d.general.cameraSwayFadeInLength, 0f, 20f,
            "Seconds for sway to reach full strength once it starts fading in.");
        _swayFadeOutLength = BindFloat(config, sway, "FadeOutLength", (float)d.general.cameraSwayFadeOutLength, 0f, 5f,
            "Seconds for sway to fade back out when you start moving.");
        _tinnitusSwayMultiplier = BindFloat(config, sway, "TinnitusSwayMultiplier", (float)d.general.tinnitusSwayMultiplier, 1f, 10f,
            "Multiplier on camera sway while you have tinnitus.");
        _exhaustionSwayMultiplier = BindFloat(config, sway, "ExhaustionSwayMultiplier", (float)d.general.exhaustionSwayMultiplier, 1f, 10f,
            "Multiplier on camera sway while exhausted.");
        _exhaustionTriggerStamina = BindFloat(config, sway, "ExhaustionTriggerStamina", (float)d.general.exhaustionTriggerStamina, 0.05f, 1f,
            "Stamina level below which exhaustion sway starts creeping in.");
        _insanitySwayMultiplier = BindFloat(config, sway, "InsanitySwayMultiplier", (float)d.general.insanitySwayMultiplier, 1f, 10f,
            "Multiplier on camera sway frequency while panicked/insane.");
        _insanityTriggerThreshold = BindFloat(config, sway, "InsanityTriggerThreshold", (float)d.general.insanityTriggerThreshold, 0f, 1f,
            "Insanity sway starts kicking in at.");
        _drunknessSwayMultiplier = BindFloat(config, sway, "DrunknessSwayMultiplier", (float)d.general.drunknessSwayMultiplier, 1f, 10f,
            "Multiplier on camera sway drift while intoxicated.");
        _criticalInjurySwayMultiplier = BindFloat(config, sway, "CriticalInjurySwayMultiplier", (float)d.general.criticalInjurySwayMultiplier, 1f, 10f,
            "Strength of the heavy near-death sway while critically injured.");
        _poisonSwayMultiplier = BindFloat(config, sway, "PoisonSwayMultiplier", (float)d.general.poisonSwayMultiplier, 1f, 10f,
            "Strength of the jittery sway while poisoned.");
        _jetpackTurbulenceIntensity = BindFloat(config, sway, "JetpackTurbulenceIntensity", (float)d.general.jetpackTurbulenceIntensity, 0f, 10f,
            "Strength of the turbulence sway while flying a jetpack.");
        _shockShakeMultiplier = BindFloat(config, sway, "ShockShakeMultiplier", (float)d.general.shockShakeMultiplier, 0f, 10f,
            "Strength of the electric jitter while being shocked.");
        _sinkingTiltStrength = BindFloat(config, sway, "SinkingTiltStrength", (float)d.general.sinkingTiltStrength, 0f, 45f,
            "Degrees of forward pitch tilt at full sink in quicksand.");

        const string shake = "5. Screen Shake";
        _shakeMaxIntensity = BindFloat(config, shake, "MaxIntensity", (float)d.general.screenShakesMaxIntensity, 0f, 10f,
            "Screen shake strength multiplying all shake.");
        _shakeMaxFrequency = BindFloat(config, shake, "MaxFrequency", (float)d.general.screenShakesMaxFrequency, 0f, 20f,
            "Screen shake speed/harshness.");
        _shakeDecay = BindFloat(config, shake, "Decay", (float)d.general.screenShakeDecay, 0.1f, 5f,
            "How fast shake fades. Trauma drained per second.");
        _explosionTrauma = BindFloat(config, shake, "ExplosionTrauma", (float)d.general.explosionTrauma, 0f, 3f,
            "Shake strength of a nearby explosion, scaled by distance.");
        _landingTrauma = BindFloat(config, shake, "LandingTrauma", (float)d.general.landingTrauma, 0f, 3f,
            "Shake strength of a hard landing, scaled by fall force and carry weight.");
        _damageTrauma = BindFloat(config, shake, "DamageTrauma", (float)d.general.damageTrauma, 0f, 3f,
            "Shake strength of taking damage, scaled by the hit size.");
        _damageKick = BindFloat(config, shake, "DamageKick", (float)d.general.damageKick, 0f, 20f,
            "Directional camera punch away from a hit, scaled by hit size.");
        _vehicleImpactTrauma = BindFloat(config, shake, "VehicleImpactTrauma", (float)d.general.vehicleImpactTrauma, 0f, 3f,
            "Shake strength of a hard cruiser crash, scaled by how fast you stopped.");
        _flashbangTrauma = BindFloat(config, shake, "FlashbangTrauma", (float)d.general.flashbangTrauma, 0f, 5f,
            "Shake strength of a Stun Grenade detonation, scaled by distance.");
        _shipTakeoffShakeStrength = BindFloat(config, shake, "ShipTakeoffShakeStrength", (float)d.general.shipTakeoffShakeStrength, 0f, 5f,
            "Camera shake that builds up as the ship blasts off. 0 disables.");
        _shipLandingShakeStrength = BindFloat(config, shake, "ShipLandingShakeStrength", (float)d.general.shipLandingShakeStrength, 0f, 5f,
            "Camera shake as the ship descends and touches down. 0 disables.");
        _weaponShakeTrauma = BindFloat(config, shake, "WeaponShakeTrauma", (float)d.general.weaponShakeTrauma, 0f, 3f,
            "Shake strength of a gunshot.");
        _weaponRecoilKick = BindFloat(config, shake, "WeaponRecoilKick", (float)d.general.weaponRecoilKick, 0f, 60f,
            "Upward recoil punch per shot.");
        _meleeWeaponShakeTrauma = BindFloat(config, shake, "MeleeWeaponShakeTrauma", (float)d.general.meleeWeaponShakeTrauma, 0f, 3f,
            "Shake strength of a shovel swing.");
        _meleeWeaponRecoilKick = BindFloat(config, shake, "MeleeWeaponRecoilKick", (float)d.general.meleeWeaponRecoilKick, 0f, 60f,
            "Upward punch per shovel swing.");
        _meleeWeaponMissMultiplier = BindFloat(config, shake, "MeleeWeaponMissMultiplier", (float)d.general.meleeWeaponMissMultiplier, 0f, 1f,
            "Multiplier on the shovel swing shake/kick if you swing but hit nothing.");
        _landingDipStrength = BindFloat(config, shake, "LandingDipStrength", (float)d.general.landingDipStrength, 0f, 20f,
            "How far the camera dips down on a hard landing.");
        _landingWeightInfluence = BindFloat(config, shake, "LandingWeightInfluence", (float)d.general.landingWeightInfluence, 0f, 2f,
            "How much carry weight hardens landings.");
        _healthConditionTriggerLimit = BindFloat(config, shake, "HealthConditionTriggerLimit", (float)d.general.healthConditionTriggerLimit, 0f, 100f,
            "Below this health, camera effects start to fade out.");

        BindContext(config, "6. Walking", _walk, d.walking);
        BindContext(config, "7. Sprinting", _sprint, d.sprinting);
        BindContext(config, "8. Cruiser", _cruiser, d.cruiser);

        const string water = "9. Water";
        _waterWadeStrength = BindFloat(config, water, "WadeStrength", (float)d.general.waterWadeStrength, 0f, 10f,
            "Strength of the heavy slosh sway while wading through water.");
        _waterSubmergedDriftStrength = BindFloat(config, water, "SubmergedDriftStrength", (float)d.general.waterSubmergedDriftStrength, 0f, 15f,
            "Strength of the slow floaty drift while fully submerged.");
        _waterSplashStrength = BindFloat(config, water, "SplashStrength", (float)d.general.waterSplashStrength, 0f, 10f,
            "Downward camera dip when entering water and when your head goes under.");

        const string leviathan = "A. Leviathan";
        _leviathanEmergeTrauma = BindFloat(config, leviathan, "EmergeTrauma", (float)d.general.leviathanEmergeTrauma, 0f, 6f,
            "Shake strength when an Earth Leviathan goes down after emerging nearby.");
        _leviathanEmergeKick = BindFloat(config, leviathan, "EmergeKick", (float)d.general.leviathanEmergeKick, 0f, 6f,
            "Directional camera punch on the emerge ground slam.");
        _leviathanProximityStrength = BindFloat(config, leviathan, "ProximityStrength", (float)d.general.leviathanProximityStrength, 0f, 5f,
            "Max degrees of continuous tremor while a leviathan is close and still burrowed.");
        _leviathanProximityRadius = BindFloat(config, leviathan, "ProximityRadius", (float)d.general.leviathanProximityRadius, 0f, 60f,
            "Distance within which the burrowed tremor is felt.");
        _leviathanWarningTremorMultiplier = BindFloat(config, leviathan, "WarningTremorMultiplier", (float)d.general.leviathanWarningTremorMultiplier, 1f, 6f,
            "How much stronger the tremor gets while a nearby worm is mid-emerge, as a warning before it bursts out.");
        _leviathanRumbleShakeMultiplier = BindFloat(config, leviathan, "RumbleShakeMultiplier", (float)d.general.leviathanRumbleShakeMultiplier, 0f, 6f,
            "Extra tremor while a nearby burrowed worm is playing its rumble sound.");
        _leviathanGrowlShakeMultiplier = BindFloat(config, leviathan, "GrowlShakeMultiplier", (float)d.general.leviathanGrowlShakeMultiplier, 0f, 6f,
            "Extra tremor while a nearby burrowed worm is playing its growl sound.");

        const string freeze = "B. Freeze";
        _freezeStrength = BindFloat(config, freeze, "Strength", (float)d.general.freezeStrength, 0f, 1f,
            "Max freeze degrees at full cold.");
        _freezeBuildSeconds = BindFloat(config, freeze, "BuildSeconds", (float)d.general.freezeBuildSeconds, 1f, 600f,
            "Seconds of being outdoor to reach full cold.");
        _freezeRecoverSeconds = BindFloat(config, freeze, "RecoverSeconds", (float)d.general.freezeRecoverSeconds, 1f, 600f,
            "Seconds to warm back up once sheltered.");
        _freezeLightReduce = BindFloat(config, freeze, "LightReduce", (float)d.general.freezeLightReduce, 0f, 1f,
            "How much a light source [ held/nearby ] reduces the freeze effect.");
        _freezeLightRadius = BindFloat(config, freeze, "LightRadius", (float)d.general.freezeLightRadius, 0f, 30f,
            "Meters within which a nearby light source object [ light pole, dropped lit flashlight, etc. ] also reduces freeze effect.");
        _freezeShipOpenDoorTarget = BindFloat(config, freeze, "ShipOpenDoorTarget", (float)d.general.freezeShipOpenDoorTarget, 0f, 1f,
            "Freeze effect reduces while in the ship with the hangar doors are still open. It fully reduces once hangar close.");

        Sync();
        config.SettingChanged += (_, _) => Sync();

        ClearOrphanedEntries(config);
        config.Save();

        config.SaveOnConfigSet = true;

        if (LethalConfigCompat.Present)
            LethalConfigCompat.Register(_entries);
    }

    private static void BindContext(ConfigFile c, string section, ContextEntries e, ConfigData.Contextual def)
    {
        e.Strafe = BindFloat(c, section, "StrafingRollFactor", (float)def.strafingRollFactor, 0f, 30f,
            "How hard the camera leans/rolls into sideways movement.");
        e.ForwardPitch = BindFloat(c, section, "ForwardVelocityPitchFactor", (float)def.forwardVelocityPitchFactor, 0f, 30f,
            "How much the camera tilts/pitches with forward/back speed.");
        e.VerticalPitch = BindFloat(c, section, "VerticalVelocityPitchFactor", (float)def.verticalVelocityPitchFactor, 0f, 30f,
            "How much the camera tilts up/down with vertical speed.");
        e.HSmooth = BindFloat(c, section, "HorizontalVelocitySmoothingFactor", (float)def.horizontalVelocitySmoothingFactor, 0f, 10f,
            "Smoothing for the strafe roll and forward-pitch effects.");
        e.VSmooth = BindFloat(c, section, "VerticalVelocitySmoothingFactor", (float)def.verticalVelocitySmoothingFactor, 0f, 10f,
            "Smoothing for the vertical velocity pitch.");
        e.MouseSmoothing = BindFloat(c, section, "CameraSmoothing", (float)def.cameraSmoothing, 0f, 5f,
            "Mouse moving smoothing.");
    }

    private static ConfigEntry<float> BindFloat(ConfigFile c, string section, string key, float def, float min, float max, string desc)
    {
        var e = c.Bind(section, key, def, new ConfigDescription(desc, new AcceptableValueRange<float>(min, max)));
        _entries.Add(e);
        return e;
    }

    private static ConfigEntry<bool> BindBool(ConfigFile c, string section, string key, bool def, string desc)
    {
        var e = c.Bind(section, key, def, desc);
        _entries.Add(e);
        return e;
    }

    private static void Sync()
    {
        var g = _data.general;
        g.masterStrength = _masterStrength.Value;
        g.contextTransitionSmoothing = _contextTransitionSmoothing.Value;
        g.maxVelocityRoll = _maxVelocityRoll.Value;
        g.maxVelocityPitch = _maxVelocityPitch.Value;
        g.enableRoll = _enableRoll.Value;
        g.enablePitch = _enablePitch.Value;
        g.enableSway = _enableSway.Value;
        g.enableScreenShake = _enableScreenShake.Value;
        g.enableLandingDip = _enableLandingDip.Value;
        g.enableWeaponShake = _enableWeaponShake.Value;
        g.enableMeleeWeaponShake = _enableMeleeWeaponShake.Value;
        g.enableTinnitusEffect = _enableTinnitusEffect.Value;
        g.enableExhaustionEffect = _enableExhaustionEffect.Value;
        g.enableInsanityEffect = _enableInsanityEffect.Value;
        g.enableDrunknessEffect = _enableDrunknessEffect.Value;
        g.enableCriticalInjuryEffect = _enableCriticalInjuryEffect.Value;
        g.enablePoisonEffect = _enablePoisonEffect.Value;
        g.enableJetpackTurbulence = _enableJetpackTurbulence.Value;
        g.enableShockEffect = _enableShockEffect.Value;
        g.enableSinkingTilt = _enableSinkingTilt.Value;
        g.enableWaterEffect = _enableWaterEffect.Value;
        g.enableLeviathanEffects = _enableLeviathanEffects.Value;
        g.enableFreezeEffect = _enableFreezeEffect.Value;
        g.enableHealthCondition = _enableHealthCondition.Value;
        g.turningRollIntensity = _turningRollIntensity.Value;
        g.turningRollAccumulation = _turningRollAccumulation.Value;
        g.turningRollSmoothing = _turningRollSmoothing.Value;
        g.cameraSwayIntensity = _swayIntensity.Value;
        g.cameraSwayFrequency = _swayFrequency.Value;
        g.cameraSwayFadeInDelay = _swayFadeInDelay.Value;
        g.cameraSwayFadeInLength = _swayFadeInLength.Value;
        g.cameraSwayFadeOutLength = _swayFadeOutLength.Value;
        g.tinnitusSwayMultiplier = _tinnitusSwayMultiplier.Value;
        g.exhaustionSwayMultiplier = _exhaustionSwayMultiplier.Value;
        g.exhaustionTriggerStamina = _exhaustionTriggerStamina.Value;
        g.insanitySwayMultiplier = _insanitySwayMultiplier.Value;
        g.insanityTriggerThreshold = _insanityTriggerThreshold.Value;
        g.drunknessSwayMultiplier = _drunknessSwayMultiplier.Value;
        g.criticalInjurySwayMultiplier = _criticalInjurySwayMultiplier.Value;
        g.poisonSwayMultiplier = _poisonSwayMultiplier.Value;
        g.jetpackTurbulenceIntensity = _jetpackTurbulenceIntensity.Value;
        g.shockShakeMultiplier = _shockShakeMultiplier.Value;
        g.sinkingTiltStrength = _sinkingTiltStrength.Value;
        g.waterWadeStrength = _waterWadeStrength.Value;
        g.waterSubmergedDriftStrength = _waterSubmergedDriftStrength.Value;
        g.waterSplashStrength = _waterSplashStrength.Value;
        g.leviathanEmergeTrauma = _leviathanEmergeTrauma.Value;
        g.leviathanEmergeKick = _leviathanEmergeKick.Value;
        g.leviathanProximityStrength = _leviathanProximityStrength.Value;
        g.leviathanProximityRadius = _leviathanProximityRadius.Value;
        g.leviathanWarningTremorMultiplier = _leviathanWarningTremorMultiplier.Value;
        g.leviathanRumbleShakeMultiplier = _leviathanRumbleShakeMultiplier.Value;
        g.leviathanGrowlShakeMultiplier = _leviathanGrowlShakeMultiplier.Value;
        g.freezeStrength = _freezeStrength.Value;
        g.freezeBuildSeconds = _freezeBuildSeconds.Value;
        g.freezeRecoverSeconds = _freezeRecoverSeconds.Value;
        g.freezeLightReduce = _freezeLightReduce.Value;
        g.freezeLightRadius = _freezeLightRadius.Value;
        g.freezeShipOpenDoorTarget = _freezeShipOpenDoorTarget.Value;
        g.screenShakesMaxIntensity = _shakeMaxIntensity.Value;
        g.screenShakesMaxFrequency = _shakeMaxFrequency.Value;
        g.screenShakeDecay = _shakeDecay.Value;
        g.explosionTrauma = _explosionTrauma.Value;
        g.landingTrauma = _landingTrauma.Value;
        g.damageTrauma = _damageTrauma.Value;
        g.damageKick = _damageKick.Value;
        g.vehicleImpactTrauma = _vehicleImpactTrauma.Value;
        g.flashbangTrauma = _flashbangTrauma.Value;
        g.shipTakeoffShakeStrength = _shipTakeoffShakeStrength.Value;
        g.shipLandingShakeStrength = _shipLandingShakeStrength.Value;
        g.weaponShakeTrauma = _weaponShakeTrauma.Value;
        g.weaponRecoilKick = _weaponRecoilKick.Value;
        g.meleeWeaponShakeTrauma = _meleeWeaponShakeTrauma.Value;
        g.meleeWeaponRecoilKick = _meleeWeaponRecoilKick.Value;
        g.meleeWeaponMissMultiplier = _meleeWeaponMissMultiplier.Value;
        g.landingDipStrength = _landingDipStrength.Value;
        g.landingWeightInfluence = _landingWeightInfluence.Value;
        g.healthConditionTriggerLimit = _healthConditionTriggerLimit.Value;

        SyncContext(_walk, _data.walking);
        SyncContext(_sprint, _data.sprinting);
        SyncContext(_cruiser, _data.cruiser);
    }

    private static void SyncContext(ContextEntries e, ConfigData.Contextual c)
    {
        c.strafingRollFactor = e.Strafe.Value;
        c.forwardVelocityPitchFactor = e.ForwardPitch.Value;
        c.verticalVelocityPitchFactor = e.VerticalPitch.Value;
        c.horizontalVelocitySmoothingFactor = e.HSmooth.Value;
        c.verticalVelocitySmoothingFactor = e.VSmooth.Value;
        c.cameraSmoothing = e.MouseSmoothing.Value;
    }

    private static void ClearOrphanedEntries(ConfigFile config)
    {
        try
        {
            PropertyInfo? orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
            if (orphanedEntriesProp != null &&
                orphanedEntriesProp.GetValue(config) is Dictionary<ConfigDefinition, string> orphanedEntries)
            {
                orphanedEntries.Clear();
                Plugin.Log.LogDebug("Cleared orphaned config entries");
            }
        }
        catch (System.Exception ex)
        {
            Plugin.Log.LogWarning($"Could not clear orphaned config entries: {ex.Message}");
        }
    }
}