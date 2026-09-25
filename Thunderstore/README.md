<div style="display: flex; gap: 10px; flex-wrap: wrap; margin-bottom: 16px;">
  <a href="https://www.codefactor.io/repository/github/se3ya/lc-cameraoverhaul"><img src="https://img.shields.io/codefactor/grade/github/se3ya/LC-CameraOverhaul?style=flat&logo=codefactor&logoColor=white&color=83E6FB&cacheSeconds=1200" alt="CodeFactor Grade"></a>
  <img src="https://img.shields.io/thunderstore/dt/seechela/Camera_Overhaul?style=flat&logo=thunderstore&logoColor=white&color=83E6FB&cacheSeconds=1200" alt="Thunderstore Downloads">
  <img src="https://img.shields.io/github/v/release/se3ya/LC-CameraOverhaul?style=flat&logo=github&logoColor=white&color=83E6FB&cacheSeconds=1200" alt="GitHub Release Version">
</div>

---
# Camera Overhaul
### **Satisfying movement through dynamic camera rotations.**
---

## Features

- Turning roll and strafing roll
- Forward and vertical velocity pitch
- Idle sway with fade-in and fade-out timing
- Weapon recoil and screen shake for shotgun fire
- Melee shake and recoil with separate miss multiplier
- Damage kick and trauma on player hit
- Hard landing dip and landing trauma
- Environmental and event shake
- Explosion and flashbang shake
- Cruiser crash impact trauma
- Ship takeoff and landing shake
- Freezing shake effect on snowy moons
- Tinnitus sway
- Exhaustion sway with stamina trigger
- Insanity sway
- Drunkness sway
- Critical injury sway
- Poison jitter
- Jetpack turbulence
- Shock jitter
- Sinking tilt
- Water effects
- Earth Leviathan effects
- Popped Jester stomp effects
- Forest Keeper stomp effects
- Bracken neck snap on kill
- Camera shake for the game's own shake events
- Flinch when something scares you
- Directional knockback from blasts and collisions
- Knockback when other player runs into you
- Walking bob
- Forward lean under a heavy load
- Jump kick
- Landing tilt
- Vanilla head bob scaling
- Lightning static charge tremor
- *Everything is configurable!*

---

## Configuration

### **1. General**

- **Master Strength** - Scales every effect
- **Context Transition Smoothing** - How smoothly tuning blends between Walking, Sprinting and Cruiser.
- **Max Velocity Roll / Max Velocity Pitch** - Safety caps in degrees on speed driven roll and pitch
- **Enable Health Condition** - Fades the movement roll and pitch out when health is low. Status effects and shake keep their strength
- **Health Condition Trigger Limit** - Health below which they start to fade

### **2. Effect Toggles**

- **Enable Roll** - Strafing lean and turning roll
- **Enable Pitch** - Forward/back and vertical velocity pitch
- **Enable Sway** - Idle sway while standing still
- **Enable Screen Shake** - Master switch for every trauma shake
- **Enable Landing Dip** - Downward punch when you land hard also the landing tilt
- **Enable Weapon Shake** - Shotgun rattle and recoil
- **Enable Melee Weapon Shake** - Shovel and knife rattle and punch

### **3. Turning Roll**

- **Intensity** - Max roll when you whip the camera left/right
- **Accumulation** - How quickly fast turning builds the lean
- **Smoothing** - How quickly it settles back to level

### **4. Camera Sway**

- **Intensity / Frequency** - Strength and speed of the idle sway
- **Fade In Delay** - Seconds of stillness before sway starts
- **Fade In Length / Fade Out Length** - Seconds to fade in and back out

### **5. Screen Shake**

- **Max Intensity / Max Frequency** - Strength and harshness of all shake
- **Decay** - Trauma drained per second
- **Explosion Trauma** - Nearby explosion, scaled by distance
- **Landing Trauma** - Hard landing, scaled by fall force and carry weight
- **Damage Trauma / Damage Kick** - Shake and directional punch when you take a hit
- **Vehicle Impact Trauma** - Hard cruiser crash, scaled by how fast you stopped
- **Flashbang Trauma** - Stun Grenade blast, scaled by distance
- **Ship Takeoff / Ship Landing Shake Strength** - Ship blasting off and touching down.
- **Weapon Shake Trauma / Weapon Recoil Kick** - Shotgun shake and upward punch per shot
- **Melee Weapon Shake Trauma / Melee Weapon Recoil Kick** - Same for a shovel swing
- **Melee Weapon Miss Multiplier** - Multiplier when you swing and hit nothing
- **Landing Dip Strength** - How far the camera dips when you land
- **Landing Tilt Strength** - How far it tilts to the side you were moving.
- **Landing Weight Influence** - How much carry weight hardens landings

### **6. Walking / 7. Sprinting / 8. Cruiser**

The same six values per movement context, blended by `ContextTransitionSmoothing`.

- **Strafing Roll Factor** - How hard the camera leans into sideways movement
- **Forward Velocity Pitch Factor** - How much it pitches with forward/back speed
- **Vertical Velocity Pitch Factor** - How much it pitches with vertical speed
- **Horizontal / Vertical Velocity Smoothing Factor** - Smoothing on those
- **Camera Smoothing** - Mouse smoothing. 0 is off and anything above adds aim lag

### **9. Water**

- **Enabled** - Slosh while wading, floaty drift while submerged
- **Wade Strength / Submerged Drift Strength** - Strength of each
- **Splash Strength** - Dip when you enter water and when your head goes under

### **A. Leviathan**

- **Enabled** - Shake and tremor from a nearby Earth Leviathan
- **Emerge Trauma / Emerge Kick** - Shake and punch on the ground slam
- **Proximity Strength / Proximity Radius** - Tremor while it is close and still burrowed
- **Warning Tremor Multiplier** - Stronger tremor while it is mid-emerge
- **Rumble / Growl Shake Multiplier** - Extra tremor on its rumble and growl sounds

### **B. Freeze**

- **Enabled** - Freezing that builds up outside on a snowy moon
- **Strength** - Max freeze degrees at full cold
- **Build Seconds / Recover Seconds** - Time to reach full cold and to warm back up
- **Light Reduce / Light Radius** - How much a held or nearby light helps, and from how far
- **Ship Open Door Target** - How much it drops in the ship with the hangar still open

### **C. Jester / D. Forest Giant**

- **Enabled** - Shake on every stomp, scaled by distance
- **Stomp Trauma** - Shake per stomp before falloff
- **Stomp Radius** - Jester only, how far the stomps are felt
- **Stomp Falloff** - How strongly it fades with distance

### **E. Bracken**

- **Enabled** - Bracken snaps your camera when it kills you
- **Snap Angle** - Degrees it snaps to the side

### **F. Walk Bob**

- **Enabled** - Camera bob timed to your footsteps
- **Pitch / Roll** - Degrees of dip per step and lean per stride.
- **Sprint Multiplier** - How much stronger it gets while sprinting
- **Carry Hunch** - Degrees the camera leans forward under a heavy load
- **Follow Game Setting** - Turns the bob off when head bobbing is off in game settings

### **G. Jump Kick**

- **Enabled / Strength** - Downward dip when you push off into a jump

### **H. Vanilla Head Bob**

- **Scale** - Scales the game's own up/down head bob.

### **I. Vanilla Shake Events**

- **Enabled** - Turns the game's own shake events into real camera shake: spike traps, Old Bird stomps, lightning, bridges, meteors and more. It replaces the flat screen shake the game does, so off means those events don't shake at all. Needs `EnableScreenShake`
- **Strength** - How hard they shake.

### **J. Fear Response**

- **Enabled** - Your camera flinches when something scares you
- **Flinch** - How hard your camera shakes the moment something scares you
- **Tremor** - Degrees of trembling right after a scare

### **K. Knockback**

- **Enabled / Strength** - Punch away from anything that pushes you: meteors, stomps, the ship magnet, cruiser hits, fox tongue
- **Player Bump** - How hard another player running into you punches the camera.

### **L. Lightning**

- **Enabled** - Trembling while you hold a metal item lightning is about to strike
- **Tremor** - Degrees of trembling right before the strike

### **M. Tinnitus / N. Exhaustion / O. Insanity / P. Drunkness / Q. Critical Injury / R. Poison / S. Jetpack / T. Shock / U. Sinking**

- **Tinnitus** - Heavy sway while your ears ring
- **Exhaustion** - Breathing sway out of stamina, with **Trigger Stamina** for when it starts
- **Insanity** - Faster sway when insanity is high, with **Trigger Threshold**.
- **Drunkness** - Floating drift on TZP
- **Critical Injury** - Pulsing near death sway
- **Poison** - Jittery sway while poisoned
- **Jetpack** - Turbulence while flying
- **Shock** - Electric jitter from a zap gun
- **Sinking** - Forward tilt as you sink into quicksand

---

## Credits

- Developed by [seeya](https://thunderstore.io/c/lethal-company/p/seechela/)
- Inspired by [Mirsario's CameraOverhaul](https://github.com/Mirsario/Minecraft-CameraOverhaul)

---

## License

Distributed under the GPL v3 License.

---

### 💖 Support

If you enjoy my work, consider [supporting](https://www.buymeacoffee.com/see_ya) me. Donations are optional but greatly appreciated.

---
