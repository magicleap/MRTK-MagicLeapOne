# Changelog
## [1.3.1] - 2021-07-01

## Bug Fixes
- Fix Unity 2020.3 compatibility

## [1.3.1] - 2021-06-21

## Bug Fixes
- Fix bug where hands become unstable after bringing them into the camera's clipping plane.

## [1.3.0] - 2021-06-17

## New Features
- Add MRTK 2.7.0 support
- Add Zero Iteration support
- Add Magic Leap eye tracking support

## Improvements
- Updated stale APIs to avoid deprecation warnings.
- Improved hand tracking stability.
- Removed unused code.
- Fixed issue where the Magic Leap controller would render on start, even though it's position was not valid.

## Bug Fixes
- Fix null reference error when using non Magic Leap specific Spatial Awareness profiles.

## Known Issues
- Scene origin starts at the floor level when using Simulation mode in Zero Iteration.
- The MRTK spatial mesh shaders require Force Multipass to be enabled. This can be done in Project Settings > Magic Leap Settings.  

## [1.2.0] - 2021-05-20

- Add Magic Leap Spatial Awareness support 

## [1.1.0] - 2021-04-20

### This is the first release of *MRTK-MagicLeap*.

- Add Magic Leap support to MRTK
