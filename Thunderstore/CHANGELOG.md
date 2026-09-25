# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.4.0] - 2026-09-26

### Added

- Camera shake on vanilla own shake events in place of vanillas flat screen shake
- Flinch when something scares you
- Directional knockback from blasts and collisions, including another player running into you
- Walking bob
- Forward lean under a heavy load
- Jump kick
- Landing tilt
- Vanilla head bob scaling
- Tremor while holding metal item lightning is about to strike
- Shake when Nutcracker or another player fires shotgun nearby
- More API methods

### Changed

- Reorganized configs. Configs migrate so it's oki
- `MasterStrength` also scales screen shake and Bracken neck snap
- Screen shake is limited smoothly instead of cut off
- Status effects and screen shake no longer fade while injured, only the movement roll and pitch do
- Sprinting blends in slower by default, no more dive when you start sprinting. Reset `ContextTransitionSmoothing` to default

### Fixed

- Camera punches kicking the wrong way at low framerate
- Camera pitch drifting while drunk or poisoned
- Flashlight beam drifting while emoting
- Turning roll building up too much at low framerate
- Camera yaw staying skewed in terminal or on lever
- Screen shake and tinnitus jumping after a frame hitch
- `ContextTransitionSmoothing` at max freezing the movement sections
- Ship takeoff shake starting and ending abruptly
- View flipping on fast flick with camera smoothing on
- Cruiser impact triggering on a frame hitch
- Earth Leviathan tremor radius, edge fade and `EmergeKick` with 0 trauma
- `DamageTrauma` default being above its own maximum
- Camera snapping when re-enabling roll or pitch
- `EnableScreenShake` also disabling ship motion and damage kick
- Other small fixes

## [1.3.0] - 2026-08-21

### Added

- Forest Keeper stomp shake
- Bracken neck snap
- Optional global config shared across all profiles
- Suppression API

### Changed

- Reorganized configs. Configs migrate so it's okay

## [1.2.1] - 2026-07-24

### Fixed

- Horizontal camera freeze while seated in the Cruiser

## [1.2.0] - 2026-07-24

### Added

- Jester stomp shake, scaled by distance

### Changed

- Improved Earth Leviathan tremor effect
- Insanity effect is disabled by default
- Insanity effect multiplier default was reduced

### Fixed

- Camera staying tilted
- Earth Leviathan tremor not always triggering when leviathan was nearby

## [1.1.0] - 2026-07-03

### Fixed

- Incompatiblity with CruiserImproved leaning and seat boost

### Added

- Freezing effects on snowy moons
  - Slowly increases while outside
  - Decreases fully when inside the facility
  - Decreases fully when inside the ship and closed hangar doors
  - Decreases slightly when inside the ship with open hangar doors
  - Decreases slightly when near a light source object/held light source object
- Earth Leviathan shake effects

## [1.0.1] - 2026-07-02

### Fixed

- Camera moving while inspecting an item

## [1.0.0] - 2026-07-01

- Initial release!
