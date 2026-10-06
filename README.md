# FrameShift for Playnite

**Per-game FPS profile manager for Playnite powered by RTSS.**

FrameShift adds a controller-friendly FPS selector to Playnite Fullscreen. Choose a frame-rate cap for a game and FrameShift stores it in that game's RTSS profile.

## Features

- 🎮 Controller-friendly FPS selector
- 🎯 Per-game profiles: **30 / 40 / 60 / 120 FPS / OFF**
- 🧩 Uses RTSS profiles instead of a global FPS cap
- 🔎 Resolves game executables from Playnite actions and install folders
- 🧠 Learns the real executable for launcher-based games after first launch
- 💾 Remembers the selected FPS for each Playnite game
- 🔌 Includes the FrameShift RTSS Bridge

## Requirements

- Windows
- Playnite **10.60**
- RivaTuner Statistics Server with `RTSSHooks64.dll`
- Visual Studio 2022 + .NET Framework **4.6.2 Developer Pack** for building

## How it works

```
Playnite game → executable → RTSS profile → FramerateLimit
```

Example:

```
Cyberpunk2077.exe      → 120 FPS
ForzaHorizon6.exe      → 60 FPS
SilentHillTownfall.exe → 40 FPS
```

The selected limit belongs to the game's executable profile, so switching between games does not require changing a global RTSS limiter.

## Installation

1. Build the solution or use a prepared release.
2. Copy the generated `FrameShift.dll`, `FrameShiftBridge.exe` and `extension.yaml` into a Playnite extension folder named `FrameShift`.
3. Run `Install-FrameShift.cmd` once as Administrator to create the bridge task.
4. Restart Playnite.
5. Open a game and select **FrameShift**.

See [INSTALL.md](INSTALL.md) for details.

## Building

Open `FrameShift.sln` in Visual Studio 2022 and build **Release**.

You can also run:

```bat
FrameShiftPlugin\build.cmd
```

See [FrameShiftPlugin/BUILD.md](FrameShiftPlugin/BUILD.md).

## Repository layout

```
FrameShift.sln
FrameShift.csproj
FrameShiftPlugin/
├─ Source/
│  ├─ FrameShiftPlugin.cs
│  ├─ FrameShiftControl.xaml
│  ├─ FrameShiftControl.xaml.cs
│  └─ FrameGamepadButton.cs
├─ RTSSBridge/
│  ├─ Program.cs
│  └─ FrameShiftBridge.csproj
├─ build.cmd
├─ Install-FrameShift.cmd
├─ Uninstall-FrameShift.cmd
└─ extension.yaml
```

The repository intentionally contains **no Fullscreen theme**. FrameShift is a Playnite plugin and RTSS integration; it does not require a custom theme.

## Launcher-based games

Steam, EA app, Epic Games, Ubisoft Connect and other launchers may hide the final game executable from Playnite. FrameShift first checks the Playnite Play action, then scans the installation directory, and can learn the actual process after the game starts.

## Status

FrameShift is based on the v13 RTSS per-profile architecture and is maintained under the FrameShift name.

## License

All rights reserved. See [LICENSE](LICENSE).
