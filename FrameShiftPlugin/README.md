# FrameShift Plugin

FrameShift stores FPS limits in RTSS profiles for individual game executables.

Example:

```
SilentHillTownfall.exe = 40 FPS
ForzaHorizon6.exe      = 60 FPS
Cyberpunk2077.exe      = 120 FPS
```

## Profile resolution

FrameShift tries, in order:

1. previously saved executable profile;
2. Playnite File/Play action;
3. conservative scan of the game's installation directory;
4. the actual process started by Playnite for launcher-based games.

Known launchers such as Steam, EA app, Epic Games and Ubisoft Connect are ignored as final profiles.

## RTSS API flow

```
LoadProfile
SetProfileProperty
SaveProfile
UpdateProfiles
```

The main property is `FramerateLimit`, with `FramerateLimitDenominator=1`.

Desktop Mode works without a custom theme (game context menu → FrameShift). In Fullscreen the FrameShift button is provided by the Toggle – Zorinel / FrameShift fork theme: https://github.com/zorinel7/Toggle-Zorinel-FrameShift
