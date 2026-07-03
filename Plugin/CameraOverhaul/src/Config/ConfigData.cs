namespace CameraOverhaul;

internal sealed class ConfigData
{
    public sealed class General
    {
        public double masterStrength = 1.0;
        public double contextTransitionSmoothing = 0.1;

        public bool enableRoll = true;      // strafing lean and turning roll
        public bool enablePitch = true;     // forward/back and vertical velocity pitch
        public bool enableSway = true;      // idle camera sway while standing still
        public bool enableScreenShake = true;       // screen shake from explosions, hard landings and taking damage.
        public bool enableLandingDip = true;        // quick downward pitch punch when player land hard
        public bool enableWeaponShake = true;       // gun fire rattle + recoil kick
        public bool enableMeleeWeaponShake = true;  // shovel swing kick
        public bool enableTinnitusEffect = true;    // camera sway/disorientation on ringing ears
        public bool enableExhaustionEffect = true;  // heavy breathing/sway when out of stamina
        public bool enableInsanityEffect = true;    // paranoid, faster sway when in dark/chased
        public bool enableDrunknessEffect = true;   // smooth floating roll when dizzy [ TZP-inhalant ]
        public bool enableCriticalInjuryEffect = true;  // heavy near death sway when critically injured
        public bool enablePoisonEffect = true;      // jittery erratic sway when poisoned
        public bool enableJetpackTurbulence = true; // unstable sway while flying a jetpack
        public bool enableShockEffect = true;       // electric jitter while zapped by a zap gun
        public bool enableSinkingTilt = true;       // forward pitch tilt while sinking in quicksand
        public bool enableWaterEffect = true;       // wade slosh + submerged buoyancy drift in water
        public bool enableLeviathanEffects = true;  // shake when an Earth Leviathan emerges or rumbles nearby
        public bool enableFreezeEffect = true;      // freeze while outside on a snowy moon
        public bool enableHealthCondition = true;      // camera effects scale down when injured

        public double turningRollAccumulation = 2.5;        // how quickly fast turning builds up the lean
        public double turningRollIntensity = 3.0;       // maximum roll the camera leans when you move the view left/right
        public double turningRollSmoothing = 1.0;       // how quickly the turning lean goes back to level once you stop turning

        public double cameraSwayIntensity = 2.0;        // strength of the idle sway
        public double cameraSwayFrequency = 0.16;       // speed of the idle sway
        public double cameraSwayFadeInDelay = 2.0;      // seconds of stillness before sway starts fading in
        public double cameraSwayFadeInLength = 5.0;     // seconds for sway to reach full strength once it starts fading in
        public double cameraSwayFadeOutLength = 0.75;   // seconds for sway to fade back out when you start moving
        public double tinnitusSwayMultiplier = 10.0; // how much stronger sway is when ears ring
        public double exhaustionSwayMultiplier = 4.0; // how much stronger sway is when exhausted
        public double exhaustionTriggerStamina = 0.4; // below what stamina level exhaustion begins
        public double insanitySwayMultiplier = 1.5; // how much stronger sway is when fully panicked
        public double insanityTriggerThreshold = 0.8; // insanity sway starts kicking in at
        public double drunknessSwayMultiplier = 6.0; // large floaty drift scaling
        public double criticalInjurySwayMultiplier = 4.0; // heavy near-death sway strength
        public double poisonSwayMultiplier = 6.0;    // jittery poisoned sway strength
        public double jetpackTurbulenceIntensity = 1.5; // turbulence sway strength while flying a jetpack
        public double shockShakeMultiplier = 3.0;    // electric jitter strength while being shocked
        public double sinkingTiltStrength = 12.0;    // degrees of forward pitch at full sink
        public double waterWadeStrength = 2.0;           // slosh sway strength while wading through water
        public double waterSubmergedDriftStrength = 5.0; // floaty buoyancy drift strength while submerged
        public double waterSplashStrength = 5.0;         // downward camera dip when entering / going under water
        public double leviathanEmergeTrauma = 2.0;       // one shake when Earth Leviathan slams down after emerging nearby
        public double leviathanEmergeKick = 3.0;         // directional camera punch on the emerge ground slam
        public double leviathanProximityStrength = 1.5;  // max degrees of tremor while a Earth Leviathan is close and still burrowed
        public double leviathanProximityRadius = 45.0;   // meters within which the burrowed tremor is felt
        public double leviathanWarningTremorMultiplier = 2.5; // how much stronger  tremor gets while a nearby Earth Leviathan is warning
        public double leviathanRumbleShakeMultiplier = 1.5;   // extra tremor while a nearby Earth Leviathan plays its rumble sound
        public double leviathanGrowlShakeMultiplier = 2.5;    // extra tremor while a nearby Earth Leviathan plays its growl sound
        public double freezeStrength = 0.4;         // max freezing degrees at full cold
        public double freezeBuildSeconds = 200.0;   // seconds outdoor to reach full cold
        public double freezeRecoverSeconds = 30.0;  // seconds to warm back up once in warm place
        public double freezeLightReduce = 0.35;     // freezing reduces while a light source is equipped or nearby
        public double freezeLightRadius = 3.0;      // meters within which a nearby light source object also reduces freezing
        public double freezeShipOpenDoorTarget = 0.3;  // freezing effect reduces while in the ship with the hangar doors are still open

        public double screenShakesMaxIntensity = 2.5;       // screen shake strength multiplies all shakes
        public double screenShakesMaxFrequency = 6.0;       // screen shake speed/harshness
        public double screenShakeDecay = 1.5;       // trauma drained per second
        public double explosionTrauma = 1.7;        // shake strength of a nearby explosion, scaled by distance.
        public double landingTrauma = 1.0;      // shake strength of a hard landing, scaled by fall force and carry weight
        public double damageTrauma = 4.0;       // shake strength of taking damage
        public double damageKick = 4.0;     // directional camera punch away from the hit
        public double vehicleImpactTrauma = 2.0;        // shake strength of a hard cruiser stops / crashes
        public double flashbangTrauma = 2.5;      // heavy shake on stun grenade blast
        public double shipTakeoffShakeStrength = 0.5;   // camera shake that builds as the ship blasts off (0 disables)
        public double shipLandingShakeStrength = 0.7;   // camera shake as the ship descends and touches down (0 disables)
        public double weaponShakeTrauma = 1.3;    // shake on firing a gun
        public double weaponRecoilKick = 30.0;      // upward recoil punch per shot
        public double meleeWeaponShakeTrauma = 1.0; // shake on swinging a shovel
        public double meleeWeaponRecoilKick = 5.0; // upward punch per shovel swing
        public double meleeWeaponMissMultiplier = 0.5; // multiplier when you swing but hit nothing
        public double landingDipStrength = 6.0;     // how far the camera dips down on a hard landing
        public double landingWeightInfluence = 0.3;     // how much carry weight hardens landings
        public double maxVelocityRoll = 12.0;
        public double maxVelocityPitch = 10.0;
        public double healthConditionTriggerLimit = 20.0;   // below this health effects fade
    }

    public sealed class Contextual
    {
        public double strafingRollFactor = 8.0;
        public double forwardVelocityPitchFactor = 2.0;
        public double verticalVelocityPitchFactor = 7.0;
        public double horizontalVelocitySmoothingFactor = 1.0;
        public double verticalVelocitySmoothingFactor = 1.0;
        public double cameraSmoothing = 0.0;

        public void CopyFrom(Contextual o)
        {
            strafingRollFactor = o.strafingRollFactor;
            forwardVelocityPitchFactor = o.forwardVelocityPitchFactor;
            verticalVelocityPitchFactor = o.verticalVelocityPitchFactor;
            horizontalVelocitySmoothingFactor = o.horizontalVelocitySmoothingFactor;
            verticalVelocitySmoothingFactor = o.verticalVelocitySmoothingFactor;
            cameraSmoothing = o.cameraSmoothing;
        }

        public Contextual Clone()
        {
            var c = new Contextual();
            c.CopyFrom(this);
            return c;
        }

        public void Lerp(Contextual a, Contextual b, double step)
        {
            strafingRollFactor = MathUtils.Lerp(a.strafingRollFactor, b.strafingRollFactor, step);
            forwardVelocityPitchFactor = MathUtils.Lerp(a.forwardVelocityPitchFactor, b.forwardVelocityPitchFactor, step);
            verticalVelocityPitchFactor = MathUtils.Lerp(a.verticalVelocityPitchFactor, b.verticalVelocityPitchFactor, step);
            horizontalVelocitySmoothingFactor = MathUtils.Lerp(a.horizontalVelocitySmoothingFactor, b.horizontalVelocitySmoothingFactor, step);
            verticalVelocitySmoothingFactor = MathUtils.Lerp(a.verticalVelocitySmoothingFactor, b.verticalVelocitySmoothingFactor, step);
            cameraSmoothing = MathUtils.Lerp(a.cameraSmoothing, b.cameraSmoothing, step);
        }
    }

    public General general = new();
    public Contextual walking = new();
    public Contextual sprinting = new();
    public Contextual cruiser = new();

    public ConfigData()
    {
        sprinting.strafingRollFactor = 10.0;
        sprinting.forwardVelocityPitchFactor = 9.5;
        sprinting.verticalVelocityPitchFactor = 8.0;
        sprinting.cameraSmoothing = 0.0;

        cruiser.strafingRollFactor = 5.0;
        cruiser.forwardVelocityPitchFactor = 3.5;
        cruiser.verticalVelocityPitchFactor = 5.0;
        cruiser.cameraSmoothing = 0.0;
    }
}