# BetterVoiceRange

A simple mod for [R.E.P.O.](https://store.steampowered.com/app/3241660/REPO/) that allows customization of how voice audio behaves based on distance and walls.

## Configuration

- **LowPassFallOffMultiplier**: This is the important value. It's the multiplier applied to voice volume over distance through walls. The higher it is, the greater the voice range. Original game value: `0.8`. Mod default value: `1.0`.
- **LowPassVolumeMultiplier**: Multiplier applied to the voice volume when behind walls. Original game value: `0.5`. Mod default value: `0.5`.
