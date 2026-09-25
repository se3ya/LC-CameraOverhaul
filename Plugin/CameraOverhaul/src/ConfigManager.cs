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
    private static int _migrated;

    private static ConfigEntry<float> _masterStrength = null!;
    private static ConfigEntry<float> _contextTransitionSmoothing = null!;
    private static ConfigEntry<float> _maxVelocityRoll = null!;
    private static ConfigEntry<float> _maxVelocityPitch = null!;

    private static ConfigEntry<bool> _enableRoll = null!;
    private static ConfigEntry<bool> _enablePitch = null!;
    private static ConfigEntry<bool> _enableSway = null!;
    private static ConfigEntry<bool> _enableScreenShake = null!;
    private static ConfigEntry<bool> _enableLandingDip = null!;
    private static ConfigEntry<bool> _enableJumpKick = null!;
    private static ConfigEntry<bool> _enableWalkBob = null!;
    private static ConfigEntry<bool> _enableVanillaShakeEvents = null!;
    private static ConfigEntry<bool> _enableFearResponse = null!;
    private static ConfigEntry<bool> _enableKnockbackKick = null!;
    private static ConfigEntry<bool> _followGameBobSetting = null!;
    private static ConfigEntry<float> _vanillaBobScale = null!;
    private static ConfigEntry<float> _vanillaShakeStrength = null!;
    private static ConfigEntry<float> _fearFlinch = null!;
    private static ConfigEntry<float> _fearTremor = null!;
    private static ConfigEntry<float> _knockbackKickStrength = null!;
    private static ConfigEntry<float> _playerBumpKick = null!;
    private static ConfigEntry<bool> _enableLightningEffect = null!;
    private static ConfigEntry<float> _lightningTremor = null!;
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
    private static ConfigEntry<bool> _enableJesterShake = null!;
    private static ConfigEntry<bool> _enableForestGiantEffect = null!;
    private static ConfigEntry<bool> _enableBrackenSnap = null!;
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
    private static ConfigEntry<float> _jesterStompTrauma = null!;
    private static ConfigEntry<float> _jesterStompRadius = null!;
    private static ConfigEntry<float> _jesterStompFalloff = null!;
    private static ConfigEntry<float> _forestGiantStompTrauma = null!;
    private static ConfigEntry<float> _forestGiantStompFalloff = null!;
    private static ConfigEntry<float> _brackenSnapAngle = null!;
    private static ConfigEntry<float> _freezeStrength = null!;
    private static ConfigEntry<float> _freezeBuildTime = null!;
    private static ConfigEntry<float> _freezeRecoverTime = null!;
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
    private static ConfigEntry<float> _landingTiltStrength = null!;
    private static ConfigEntry<float> _jumpKickStrength = null!;
    private static ConfigEntry<float> _walkBobPitch = null!;
    private static ConfigEntry<float> _walkBobRoll = null!;
    private static ConfigEntry<float> _walkBobSprintMultiplier = null!;
    private static ConfigEntry<float> _carryHunch = null!;
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
        Dictionary<ConfigDefinition, string> preBind = SnapshotConfigEntries(config);

        const string general = "1. General";
        _masterStrength = BindFloat(config, general, "MasterStrength", (float)d.general.masterStrength, 0f, 3f,
            "Scales every effect.");
        _contextTransitionSmoothing = BindFloat(config, general, "ContextTransitionSmoothing", (float)d.general.contextTransitionSmoothing, 0f, 0.99f,
            "How smoothly tuning blends between the '6. Walking', '7. Sprinting' and '8. Cruiser' sections.");
        _maxVelocityRoll = BindFloat(config, general, "MaxVelocityRoll", (float)d.general.maxVelocityRoll, 0f, 90f,
            "Hard cap in degrees on speed driven roll, so fast strafe can never throw camera further than this.");
        _maxVelocityPitch = BindFloat(config, general, "MaxVelocityPitch", (float)d.general.maxVelocityPitch, 0f, 90f,
            "Same cap for speed driven pitch.");
        _enableHealthCondition = BindBool(config, general, "EnableHealthCondition", d.general.enableHealthCondition,
            "Fades the movement roll and pitch out the closer you are to dying. Status effects and screen shake keep their strength.");
        _healthConditionTriggerLimit = BindFloat(config, general, "HealthConditionTriggerLimit", (float)d.general.healthConditionTriggerLimit, 0f, 100f,
            "Health fade starts at.");

        const string toggles = "2. Effect Toggles";
        _enableRoll = BindBool(config, toggles, "EnableRoll", d.general.enableRoll,
            "Strafing lean, tuned in sections 6 to 8, and turning roll, tuned in section 3.");
        _enablePitch = BindBool(config, toggles, "EnablePitch", d.general.enablePitch,
            "Forward/back and vertical velocity pitch. Same three sections tune it.");
        _enableSway = BindBool(config, toggles, "EnableSway", d.general.enableSway,
            "Idle camera sway while standing still. '4. Camera Sway' has the timings.");
        _enableScreenShake = BindBool(config, toggles, "EnableScreenShake", d.general.enableScreenShake,
            "Master switch for every trauma shake: explosions, landings, damage, vanilla shake events. Off also removes the game's own screen shake, so those events go completely still.");
        _enableLandingDip = BindBool(config, toggles, "EnableLandingDip", d.general.enableLandingDip,
            "Quick downward pitch punch and the sideways tilt when you land hard. Turning this off disables both.");
        _enableWeaponShake = BindBool(config, toggles, "EnableWeaponShake", d.general.enableWeaponShake,
            "Rattle and upward recoil kick when firing a gun. Someone else shot comes through 'I. Vanilla Shake Events' instead.");
        _enableMeleeWeaponShake = BindBool(config, toggles, "EnableMeleeWeaponShake", d.general.enableMeleeWeaponShake,
            "Rattle and upward punch when you swing melee weapon.");

        const string turning = "3. Turning Roll";
        _turningRollIntensity = BindFloat(config, turning, "Intensity", (float)d.general.turningRollIntensity, 0f, 5f,
            "How far the camera leans when you whip the camera left/right. The default 3 lands around 3.75 degrees.");
        _turningRollAccumulation = BindFloat(config, turning, "Accumulation", (float)d.general.turningRollAccumulation, 0f, 5f,
            "How quickly fast turning builds the lean up. Higher = reaches full lean sooner.");
        _turningRollSmoothing = BindFloat(config, turning, "Smoothing", (float)d.general.turningRollSmoothing, 0f, 5f,
            "How quickly the lean settles back to level once you stop turning.");

        const string sway = "4. Camera Sway";
        _swayIntensity = BindFloat(config, sway, "Intensity", (float)d.general.cameraSwayIntensity, 0f, 5f,
            "Strength of the idle sway while standing still.");
        _swayFrequency = BindFloat(config, sway, "Frequency", (float)d.general.cameraSwayFrequency, 0f, 2f,
            "Speed of the idle sway.");
        _swayFadeInDelay = BindFloat(config, sway, "FadeInDelay", (float)d.general.cameraSwayFadeInDelay, 0f, 5f,
            "Seconds of stillness before the sway starts fading in.");
        _swayFadeInLength = BindFloat(config, sway, "FadeInLength", (float)d.general.cameraSwayFadeInLength, 0f, 20f,
            "Seconds for the sway to reach full strength once it starts.");
        _swayFadeOutLength = BindFloat(config, sway, "FadeOutLength", (float)d.general.cameraSwayFadeOutLength, 0f, 5f,
            "Seconds to fade back out when you move.");

        const string shake = "5. Screen Shake";
        _shakeMaxIntensity = BindFloat(config, shake, "MaxIntensity", (float)d.general.screenShakesMaxIntensity, 0f, 10f,
            "Degrees of shake at trauma 1. Bigger hits square that, up to a 12 degree ceiling.");
        _shakeMaxFrequency = BindFloat(config, shake, "MaxFrequency", (float)d.general.screenShakesMaxFrequency, 0f, 20f,
            "Screen shake speed. Higher is harsher.");
        _shakeDecay = BindFloat(config, shake, "Decay", (float)d.general.screenShakeDecay, 0.1f, 5f,
            "Trauma drained per second.");
        _explosionTrauma = BindFloat(config, shake, "ExplosionTrauma", (float)d.general.explosionTrauma, 0f, 3f,
            "Shake of nearby explosion, scaled by distance.");
        _landingTrauma = BindFloat(config, shake, "LandingTrauma", (float)d.general.landingTrauma, 0f, 3f,
            "Shake of hard landing, scaled by fall force and carry weight. 'LandingWeightInfluence' sets how much the weight counts.");
        _damageTrauma = BindFloat(config, shake, "DamageTrauma", (float)d.general.damageTrauma, 0f, 3f,
            "Shake of taking a hit, scaled by how big the hit was.");
        _damageKick = BindFloat(config, shake, "DamageKick", (float)d.general.damageKick, 0f, 20f,
            "Directional punch away from a hit, scaled by hit size.");
        _vehicleImpactTrauma = BindFloat(config, shake, "VehicleImpactTrauma", (float)d.general.vehicleImpactTrauma, 0f, 3f,
            "Shake of a hard cruiser crash, scaled by how fast you stopped.");
        _flashbangTrauma = BindFloat(config, shake, "FlashbangTrauma", (float)d.general.flashbangTrauma, 0f, 3f,
            "Shake of Stun Grenade going off, scaled by distance.");
        _shipTakeoffShakeStrength = BindFloat(config, shake, "ShipTakeoffShakeStrength", (float)d.general.shipTakeoffShakeStrength, 0f, 5f,
            "Shake that builds as the ship takes off.");
        _shipLandingShakeStrength = BindFloat(config, shake, "ShipLandingShakeStrength", (float)d.general.shipLandingShakeStrength, 0f, 5f,
            "Shake as the ship lands.");
        _weaponShakeTrauma = BindFloat(config, shake, "WeaponShakeTrauma", (float)d.general.weaponShakeTrauma, 0f, 3f,
            "Shake of a gunshot.");
        _weaponRecoilKick = BindFloat(config, shake, "WeaponRecoilKick", (float)d.general.weaponRecoilKick, 0f, 60f,
            "Upward recoil punch per shot.");
        _meleeWeaponShakeTrauma = BindFloat(config, shake, "MeleeWeaponShakeTrauma", (float)d.general.meleeWeaponShakeTrauma, 0f, 3f,
            "Shake of a melee swing.");
        _meleeWeaponRecoilKick = BindFloat(config, shake, "MeleeWeaponRecoilKick", (float)d.general.meleeWeaponRecoilKick, 0f, 60f,
            "Upward punch per swing.");
        _meleeWeaponMissMultiplier = BindFloat(config, shake, "MeleeWeaponMissMultiplier", (float)d.general.meleeWeaponMissMultiplier, 0f, 1f,
            "Multiplier on the swing shake and kick when missing the hit.");
        _landingDipStrength = BindFloat(config, shake, "LandingDipStrength", (float)d.general.landingDipStrength, 0f, 20f,
            "How far the camera dips on a hard landing.");
        _landingTiltStrength = BindFloat(config, shake, "LandingTiltStrength", (float)d.general.landingTiltStrength, 0f, 20f,
            "How far the camera tilts toward the side you were moving when you land.");
        _landingWeightInfluence = BindFloat(config, shake, "LandingWeightInfluence", (float)d.general.landingWeightInfluence, 0f, 2f,
            "How much carry weight hardens a landing.");

        BindContext(config, "6. Walking", _walk, d.walking);
        BindContext(config, "7. Sprinting", _sprint, d.sprinting);
        BindContext(config, "8. Cruiser", _cruiser, d.cruiser);

        const string water = "9. Water";
        _enableWaterEffect = BindBool(config, water, "Enabled", d.general.enableWaterEffect,
            "Sloshy sway while wading and floaty drift while submerged in water.");
        _waterWadeStrength = BindFloat(config, water, "WadeStrength", (float)d.general.waterWadeStrength, 0f, 10f,
            "Strength of the slosh while wading.");
        _waterSubmergedDriftStrength = BindFloat(config, water, "SubmergedDriftStrength", (float)d.general.waterSubmergedDriftStrength, 0f, 15f,
            "Strength of the slow drift while fully under.");
        _waterSplashStrength = BindFloat(config, water, "SplashStrength", (float)d.general.waterSplashStrength, 0f, 10f,
            "Half a dip when you enter water and a full one when your head goes under.");

        const string leviathan = "A. Leviathan";
        _enableLeviathanEffects = BindBool(config, leviathan, "Enabled", d.general.enableLeviathanEffects,
            "Shake when an Earth Leviathan bursts out/dives back, plus tremor while it's close and still buried.");
        _leviathanEmergeTrauma = BindFloat(config, leviathan, "EmergeTrauma", (float)d.general.leviathanEmergeTrauma, 0f, 3f,
            "Shake when it slams down after emerging nearby.");
        _leviathanEmergeKick = BindFloat(config, leviathan, "EmergeKick", (float)d.general.leviathanEmergeKick, 0f, 6f,
            "Downward punch on that ground slam.");
        _leviathanProximityStrength = BindFloat(config, leviathan, "ProximityStrength", (float)d.general.leviathanProximityStrength, 0f, 5f,
            "Degrees of tremor while one is buried nearby, before the multipliers below raise it.");
        _leviathanProximityRadius = BindFloat(config, leviathan, "ProximityRadius", (float)d.general.leviathanProximityRadius, 0f, 60f,
            "Meters within which you feel the buried tremor.");
        _leviathanWarningTremorMultiplier = BindFloat(config, leviathan, "WarningTremorMultiplier", (float)d.general.leviathanWarningTremorMultiplier, 1f, 6f,
            "How much stronger the tremor gets while it's mid emerge.");
        _leviathanRumbleShakeMultiplier = BindFloat(config, leviathan, "RumbleShakeMultiplier", (float)d.general.leviathanRumbleShakeMultiplier, 0f, 6f,
            "Extra tremor while buried worm plays its rumble sound.");
        _leviathanGrowlShakeMultiplier = BindFloat(config, leviathan, "GrowlShakeMultiplier", (float)d.general.leviathanGrowlShakeMultiplier, 0f, 6f,
            "Extra tremor for growl.");

        const string freeze = "B. Freeze";
        _enableFreezeEffect = BindBool(config, freeze, "Enabled", d.general.enableFreezeEffect,
            "Freezing that builds while outside on a snowy moon.");
        _freezeStrength = BindFloat(config, freeze, "Strength", (float)d.general.freezeStrength, 0f, 1f,
            "Most degrees of freeze at full cold.");
        _freezeBuildTime = BindFloat(config, freeze, "BuildSeconds", (float)d.general.freezeBuildTime, 1f, 600f,
            "Seconds outside to reach full cold.");
        _freezeRecoverTime = BindFloat(config, freeze, "RecoverSeconds", (float)d.general.freezeRecoverTime, 1f, 600f,
            "Seconds to warm back up once you're sheltered.");
        _freezeLightReduce = BindFloat(config, freeze, "LightReduce", (float)d.general.freezeLightReduce, 0f, 1f,
            "How much a light source [ held/nearby ] cuts the freeze.");
        _freezeLightRadius = BindFloat(config, freeze, "LightRadius", (float)d.general.freezeLightRadius, 0f, 30f,
            "Meters within which a nearby light [ light pole, dropped flashlight ] counts too.");
        _freezeShipOpenDoorTarget = BindFloat(config, freeze, "ShipOpenDoorTarget", (float)d.general.freezeShipOpenDoorTarget, 0f, 1f,
            "How much freeze is left while in the ship with the hangar doors still open. It clears fully once they close.");

        const string jester = "C. Jester";
        _enableJesterShake = BindBool(config, jester, "Enabled", d.general.enableJesterShake,
            "Shake on every stomp of a popped Jester, scaled by distance.");
        _jesterStompTrauma = BindFloat(config, jester, "StompTrauma", (float)d.general.jesterStompTrauma, 0f, 3f,
            "Shake per stomp before distance falloff.");
        _jesterStompRadius = BindFloat(config, jester, "StompRadius", (float)d.general.jesterStompRadius, 0f, 40f,
            "Meters within which its stomps reach you.");
        _jesterStompFalloff = BindFloat(config, jester, "StompFalloff", (float)d.general.jesterStompFalloff, 1f, 4f,
            "How sharply the stomp shake fades with distance. Higher = drops off faster.");

        const string forestGiant = "D. Forest Giant";
        _enableForestGiantEffect = BindBool(config, forestGiant, "Enabled", d.general.enableForestGiantEffect,
            "Shake on every step of a Forest Keeper, scaled by distance.");
        _forestGiantStompTrauma = BindFloat(config, forestGiant, "StompTrauma", (float)d.general.forestGiantStompTrauma, 0f, 3f,
            "Shake strength per stomp of chasing Forest Keeper.");
        _forestGiantStompFalloff = BindFloat(config, forestGiant, "StompFalloff", (float)d.general.forestGiantStompFalloff, 1f, 4f,
            "How sharply it fades toward the edge of hearing.");

        const string bracken = "E. Bracken";
        _enableBrackenSnap = BindBool(config, bracken, "Enabled", d.general.enableBrackenSnap,
            "Bracken snaps camera to the side when it gets you.");
        _brackenSnapAngle = BindFloat(config, bracken, "SnapAngle", (float)d.general.brackenSnapAngle, 0f, 180f,
            "Degrees the camera is snapped.");

        const string walkBob = "F. Walk Bob";
        _enableWalkBob = BindBool(config, walkBob, "Enabled", d.general.enableWalkBob,
            "Camera bob.");
        _walkBobPitch = BindFloat(config, walkBob, "Pitch", (float)d.general.walkBobPitch, 0f, 3f,
            "Degrees the camera dips with each step.");
        _walkBobRoll = BindFloat(config, walkBob, "Roll", (float)d.general.walkBobRoll, 0f, 3f,
            "Degrees the camera leans side to side over a stride.");
        _walkBobSprintMultiplier = BindFloat(config, walkBob, "SprintMultiplier", (float)d.general.walkBobSprintMultiplier, 1f, 3f,
            "How much stronger the bob gets while sprinting.");
        _carryHunch = BindFloat(config, walkBob, "CarryHunch", (float)d.general.carryHunch, 0f, 5f,
            "Degrees the camera leans forward under a heavy load.");
        _followGameBobSetting = BindBool(config, walkBob, "FollowGameSetting", d.general.followGameBobSetting,
            "Turns the bob off when head bobbing is off in the game's own settings.");

        const string jumpKick = "G. Jump Kick";
        _enableJumpKick = BindBool(config, jumpKick, "Enabled", d.general.enableJumpKick,
            "Quick downward dip when jumping.");
        _jumpKickStrength = BindFloat(config, jumpKick, "Strength", (float)d.general.jumpKickStrength, 0f, 10f,
            "How far the camera dips. Sections 6 to 8 add their own dip from vertical speed.");

        const string gameBob = "H. Vanilla Head Bob";
        _vanillaBobScale = BindFloat(config, gameBob, "Scale", (float)d.general.vanillaBobScale, 0f, 1f,
            "Scales games own up/down head bob.");

        const string vanillaShake = "I. Vanilla Shake Events";
        _enableVanillaShakeEvents = BindBool(config, vanillaShake, "Enabled", d.general.enableVanillaShakeEvents,
            "Turns games own shake events into real camera shake: spike traps, Old Bird stomps, lightning, bridges, meteors. It replaces the flat screen slide the game does, so off means those events don't shake at all. REQUIRES 'EnableScreenShake'.");
        _vanillaShakeStrength = BindFloat(config, vanillaShake, "Strength", (float)d.general.vanillaShakeStrength, 0f, 3f,
            "How hard they shake.");

        const string fear = "J. Fear Response";
        _enableFearResponse = BindBool(config, fear, "Enabled", d.general.enableFearResponse,
            "Your camera flinches when someone scares you.");
        _fearFlinch = BindFloat(config, fear, "Flinch", (float)d.general.fearFlinch, 0f, 10f,
            "How hard the camera flinches.");
        _fearTremor = BindFloat(config, fear, "Tremor", (float)d.general.fearTremor, 0f, 1f,
            "Degrees of trembling after a scare.");

        const string knockback = "K. Knockback";
        _enableKnockbackKick = BindBool(config, knockback, "Enabled", d.general.enableKnockbackKick,
            "Shakes the camera away from anything that shoves you: meteor blasts, Old Bird stomps, ship magnet, cruiser hits, the fox tongue.");
        _knockbackKickStrength = BindFloat(config, knockback, "Strength", (float)d.general.knockbackKickStrength, 0f, 15f,
            "How hard a full strength shove punches. Most real shoves land well under that.");
        _playerBumpKick = BindFloat(config, knockback, "PlayerBump", (float)d.general.playerBumpKick, 0f, 15f,
            "How hard another player running into you shakes the camera. Needs 3 m/s of speed.");

        const string lightning = "L. Lightning";
        _enableLightningEffect = BindBool(config, lightning, "Enabled", d.general.enableLightningEffect,
            "Your camera trembles while you hold a metal item lightning is about to hit.");
        _lightningTremor = BindFloat(config, lightning, "Tremor", (float)d.general.lightningTremor, 0f, 3f,
            "Degrees of trembling.");

        const string tinnitus = "M. Tinnitus";
        _enableTinnitusEffect = BindBool(config, tinnitus, "Enabled", d.general.enableTinnitusEffect,
            "Heavy sway while your ears ring after a blast.");
        _tinnitusSwayMultiplier = BindFloat(config, tinnitus, "SwayMultiplier", (float)d.general.tinnitusSwayMultiplier, 1f, 10f,
            "Sway while your ears ring.");

        const string exhaustion = "N. Exhaustion";
        _enableExhaustionEffect = BindBool(config, exhaustion, "Enabled", d.general.enableExhaustionEffect,
            "Breathing sway as your stamina runs out.");
        _exhaustionSwayMultiplier = BindFloat(config, exhaustion, "SwayMultiplier", (float)d.general.exhaustionSwayMultiplier, 1f, 10f,
            "Strength of sway.");
        _exhaustionTriggerStamina = BindFloat(config, exhaustion, "TriggerStamina", (float)d.general.exhaustionTriggerStamina, 0.05f, 1f,
            "Stamina level below which exhaustion sway starts creeping in.");

        const string insanity = "O. Insanity";
        _enableInsanityEffect = BindBool(config, insanity, "Enabled", d.general.enableInsanityEffect,
            "Faster sway as your insanity climbs.");
        _insanitySwayMultiplier = BindFloat(config, insanity, "SwayMultiplier", (float)d.general.insanitySwayMultiplier, 1f, 10f,
            "Strength of the insanity sway.");
        _insanityTriggerThreshold = BindFloat(config, insanity, "TriggerThreshold", (float)d.general.insanityTriggerThreshold, 0f, 1f,
            "Fraction of max insanity the sway starts at.");

        const string drunkness = "P. Drunkness";
        _enableDrunknessEffect = BindBool(config, drunkness, "Enabled", d.general.enableDrunknessEffect,
            "Smooth floating drift while you're on TZP.");
        _drunknessSwayMultiplier = BindFloat(config, drunkness, "SwayMultiplier", (float)d.general.drunknessSwayMultiplier, 1.5f, 12f,
            "Strength of the drift.");

        const string critical = "Q. Critical Injury";
        _enableCriticalInjuryEffect = BindBool(config, critical, "Enabled", d.general.enableCriticalInjuryEffect,
            "Pulsing sway while you're close to death.");
        _criticalInjurySwayMultiplier = BindFloat(config, critical, "SwayMultiplier", (float)d.general.criticalInjurySwayMultiplier, 1f, 10f,
            "Strength of that pulse.");

        const string poison = "R. Poison";
        _enablePoisonEffect = BindBool(config, poison, "Enabled", d.general.enablePoisonEffect,
            "Jittery sway while poisoned.");
        _poisonSwayMultiplier = BindFloat(config, poison, "SwayMultiplier", (float)d.general.poisonSwayMultiplier, 1f, 10f,
            "Strength of the jitter.");

        const string jetpack = "S. Jetpack";
        _enableJetpackTurbulence = BindBool(config, jetpack, "Enabled", d.general.enableJetpackTurbulence,
            "Turbulence while you're flying a jetpack.");
        _jetpackTurbulenceIntensity = BindFloat(config, jetpack, "Turbulence", (float)d.general.jetpackTurbulenceIntensity, 0f, 10f,
            "Strength of the turbulence.");

        const string shock = "T. Shock";
        _enableShockEffect = BindBool(config, shock, "Enabled", d.general.enableShockEffect,
            "Electric jitter while a zap gun zaps.");
        _shockShakeMultiplier = BindFloat(config, shock, "ShakeMultiplier", (float)d.general.shockShakeMultiplier, 0f, 10f,
            "Strength of the jitter.");

        const string sinking = "U. Sinking";
        _enableSinkingTilt = BindBool(config, sinking, "Enabled", d.general.enableSinkingTilt,
            "Forward tilt as you sink into quicksand.");
        _sinkingTiltStrength = BindFloat(config, sinking, "TiltStrength", (float)d.general.sinkingTiltStrength, 0f, 45f,
            "Degrees of forward tilt once you're fully sunk.");

        MigrateToFeatureLayout(preBind);

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
            "How hard the camera leans into sideways movement. Each of the three sections carries its own value.");
        e.ForwardPitch = BindFloat(c, section, "ForwardVelocityPitchFactor", (float)def.forwardVelocityPitchFactor, 0f, 30f,
            "How much the camera pitches with forward and back speed.");
        e.VerticalPitch = BindFloat(c, section, "VerticalVelocityPitchFactor", (float)def.verticalVelocityPitchFactor, 0f, 30f,
            "How much the camera pitches with vertical speed.");
        e.HSmooth = BindFloat(c, section, "HorizontalVelocitySmoothingFactor", (float)def.horizontalVelocitySmoothingFactor, 0f, 10f,
            "Smoothing for the strafe roll and forward pitch.");
        e.VSmooth = BindFloat(c, section, "VerticalVelocitySmoothingFactor", (float)def.verticalVelocitySmoothingFactor, 0f, 10f,
            "Smoothing for the vertical velocity pitch.");
        e.MouseSmoothing = BindFloat(c, section, "CameraSmoothing", (float)def.cameraSmoothing, 0f, 5f,
            "Mouse smoothing. 0 is raw input and anything above it adds aim lag.");
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
        g.enableJumpKick = _enableJumpKick.Value;
        g.enableWalkBob = _enableWalkBob.Value;
        g.enableVanillaShakeEvents = _enableVanillaShakeEvents.Value;
        g.enableFearResponse = _enableFearResponse.Value;
        g.enableKnockbackKick = _enableKnockbackKick.Value;
        g.followGameBobSetting = _followGameBobSetting.Value;
        g.vanillaBobScale = _vanillaBobScale.Value;
        g.vanillaShakeStrength = _vanillaShakeStrength.Value;
        g.fearFlinch = _fearFlinch.Value;
        g.fearTremor = _fearTremor.Value;
        g.knockbackKickStrength = _knockbackKickStrength.Value;
        g.playerBumpKick = _playerBumpKick.Value;
        g.enableLightningEffect = _enableLightningEffect.Value;
        g.lightningTremor = _lightningTremor.Value;
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
        g.enableJesterShake = _enableJesterShake.Value;
        g.enableForestGiantEffect = _enableForestGiantEffect.Value;
        g.enableBrackenSnap = _enableBrackenSnap.Value;
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
        g.jesterStompTrauma = _jesterStompTrauma.Value;
        g.jesterStompRadius = _jesterStompRadius.Value;
        g.jesterStompFalloff = _jesterStompFalloff.Value;
        g.forestGiantStompTrauma = _forestGiantStompTrauma.Value;
        g.forestGiantStompFalloff = _forestGiantStompFalloff.Value;
        g.brackenSnapAngle = _brackenSnapAngle.Value;
        g.freezeStrength = _freezeStrength.Value;
        g.freezeBuildTime = _freezeBuildTime.Value;
        g.freezeRecoverTime = _freezeRecoverTime.Value;
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
        g.landingTiltStrength = _landingTiltStrength.Value;
        g.jumpKickStrength = _jumpKickStrength.Value;
        g.walkBobPitch = _walkBobPitch.Value;
        g.walkBobRoll = _walkBobRoll.Value;
        g.walkBobSprintMultiplier = _walkBobSprintMultiplier.Value;
        g.carryHunch = _carryHunch.Value;
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

    private static Dictionary<ConfigDefinition, string> SnapshotConfigEntries(ConfigFile config)
    {
        var snapshot = new Dictionary<ConfigDefinition, string>();
        try
        {
            PropertyInfo? orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
            if (orphanedEntriesProp != null &&
                orphanedEntriesProp.GetValue(config) is Dictionary<ConfigDefinition, string> orphanedEntries)
            {
                foreach (KeyValuePair<ConfigDefinition, string> pair in orphanedEntries)
                    snapshot[pair.Key] = pair.Value;
            }
        }
        catch (System.Exception ex)
        {
            Plugin.Log.LogWarning($"Could not read existing config for migration: {ex.Message}");
        }
        return snapshot;
    }

    private static void MigrateToFeatureLayout(Dictionary<ConfigDefinition, string> preBind)
    {
        if (preBind.Count == 0) return;

        _migrated = 0;
        MigrateEntry(preBind, "2. Effect Toggles", "EnableWaterEffect", _enableWaterEffect);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableLeviathanEffects", _enableLeviathanEffects);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableFreezeEffect", _enableFreezeEffect);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableJesterShake", _enableJesterShake);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableForestGiantEffect", _enableForestGiantEffect);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableBrackenSnap", _enableBrackenSnap);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableHealthCondition", _enableHealthCondition);
        MigrateEntry(preBind, "5. Screen Shake", "HealthConditionTriggerLimit", _healthConditionTriggerLimit);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableTinnitusEffect", _enableTinnitusEffect);
        MigrateEntry(preBind, "4. Camera Sway", "TinnitusSwayMultiplier", _tinnitusSwayMultiplier);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableExhaustionEffect", _enableExhaustionEffect);
        MigrateEntry(preBind, "4. Camera Sway", "ExhaustionSwayMultiplier", _exhaustionSwayMultiplier);
        MigrateEntry(preBind, "4. Camera Sway", "ExhaustionTriggerStamina", _exhaustionTriggerStamina);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableInsanityEffect", _enableInsanityEffect);
        MigrateEntry(preBind, "4. Camera Sway", "InsanitySwayMultiplier", _insanitySwayMultiplier);
        MigrateEntry(preBind, "4. Camera Sway", "InsanityTriggerThreshold", _insanityTriggerThreshold);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableDrunknessEffect", _enableDrunknessEffect);
        MigrateEntry(preBind, "4. Camera Sway", "DrunknessSwayMultiplier", _drunknessSwayMultiplier);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableCriticalInjuryEffect", _enableCriticalInjuryEffect);
        MigrateEntry(preBind, "4. Camera Sway", "CriticalInjurySwayMultiplier", _criticalInjurySwayMultiplier);
        MigrateEntry(preBind, "2. Effect Toggles", "EnablePoisonEffect", _enablePoisonEffect);
        MigrateEntry(preBind, "4. Camera Sway", "PoisonSwayMultiplier", _poisonSwayMultiplier);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableJetpackTurbulence", _enableJetpackTurbulence);
        MigrateEntry(preBind, "4. Camera Sway", "JetpackTurbulenceIntensity", _jetpackTurbulenceIntensity);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableShockEffect", _enableShockEffect);
        MigrateEntry(preBind, "4. Camera Sway", "ShockShakeMultiplier", _shockShakeMultiplier);
        MigrateEntry(preBind, "2. Effect Toggles", "EnableSinkingTilt", _enableSinkingTilt);
        MigrateEntry(preBind, "4. Camera Sway", "SinkingTiltStrength", _sinkingTiltStrength);

        if (_migrated > 0)
            Plugin.Log.LogInfo($"Migrated {_migrated} config entries into the new layout.");
    }

    private static void MigrateEntry<T>(Dictionary<ConfigDefinition, string> preBind, string oldSection, string oldKey, ConfigEntry<T> target)
    {
        if (preBind.ContainsKey(target.Definition)) return;
        if (!preBind.TryGetValue(new ConfigDefinition(oldSection, oldKey), out string raw)) return;

        try
        {
            target.Value = (T)TomlTypeConverter.ConvertToValue(raw, typeof(T));
            _migrated++;
        }
        catch (System.Exception ex)
        {
            Plugin.Log.LogWarning($"Could not migrate {oldSection}/{oldKey}: {ex.Message}");
        }
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
