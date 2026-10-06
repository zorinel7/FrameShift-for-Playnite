# FrameShift for Playnite

**Per-game FPS profile manager for Playnite powered by RivaTuner Statistics Server (RTSS).**

FrameShift adds a controller-friendly FPS selector to Playnite Fullscreen. Pick a frame-rate limit for a game and FrameShift stores it in the RTSS profile belonging to the game's executable.

> **Project status:** source repository ready. A Windows/Visual Studio build is required to produce the binary release files (`FrameShift.dll` and `FrameShiftBridge.exe`).

## ✨ Features
- 🎮 Controller-friendly UI for Playnite Fullscreen
- 🎯 Per-game FPS profiles
- ⚡ 30 / 40 / 60 / 120 FPS / OFF
- 🧩 RTSS per-application profiles
- 🔎 Automatic executable resolution
- 🧠 Learns the real executable after launcher-based games start
- 💾 Remembers the selected FPS for each game
- 🚫 Ignores common launchers as final profiles
- 🔌 Dedicated FrameShift RTSS Bridge
- 🖥️ No custom Playnite theme required

## 🎮 How it works
```text
Playnite game
     ↓
Game executable
     ↓
RTSS application profile
     ↓
FramerateLimit
```

Example:
```text
Cyberpunk2077.exe      → 120 FPS
ForzaHorizon6.exe      → 60 FPS
SilentHillTownfall.exe → 40 FPS
```

## 📋 Available profiles
| Mode | Purpose |
|---|---|
| **30 FPS** | Cinematic / low-power profile |
| **40 FPS** | Balanced console-style profile |
| **60 FPS** | Standard gaming profile |
| **120 FPS** | High-refresh gaming |
| **OFF** | Disable the selected limit |

## 🔎 Executable detection
FrameShift checks, in order:
1. Previously saved executable profile.
2. Playnite Play/File action.
3. Game installation directory.
4. Actual process started by Playnite when necessary.

Known launcher processes such as Steam, EA app, Epic Games and Ubisoft Connect are not treated as final game profiles.

## 🛠️ Requirements
### Runtime
- Windows 10/11
- Playnite 10.60
- RivaTuner Statistics Server
- RTSS installation containing `RTSSHooks64.dll`

### Building from source
- Windows
- Visual Studio 2022
- .NET Framework 4.6.2 Developer Pack
- .NET desktop development workload
- Playnite SDK reference

## 📦 Installation from a Release
1. Download the `FrameShift-x.x.x.zip` asset from GitHub Releases.
2. Close Playnite.
3. Extract the package.
4. Copy the extension files to `%AppData%\Playnite\Extensions\FrameShift\`.
5. Run `Install-FrameShift.cmd` as Administrator.
6. Start Playnite.
7. Open a game and select **FrameShift**.

Expected release contents:
```text
FrameShift.dll
FrameShiftBridge.exe
extension.yaml
Install-FrameShift.cmd
Uninstall-FrameShift.cmd
INSTALL.md
```

## 🔨 Building
Open `FrameShift.sln` in Visual Studio 2022 and build **Release / Any CPU**.

Or run:
```bat
FrameShiftPlugin\build.cmd
```

The solution contains `FrameShift.csproj` and `FrameShiftPlugin/RTSSBridge/FrameShiftBridge.csproj`.

The build produces `FrameShift.dll` and `FrameShiftBridge.exe`.

### Why binaries are not committed
The plugin targets .NET Framework/WPF and depends on the Playnite SDK. The repository contains reproducible source code; compiled binaries belong in GitHub Release assets.

## 🧩 RTSS Bridge
The bridge communicates with RTSS and updates the selected application's profile using functions such as `LoadProfile`, `GetProfileProperty`, `SetProfileProperty`, `SaveProfile` and `UpdateProfiles`.

The primary property is `FramerateLimit`, with `FramerateLimitDenominator = 1`.

## 🧪 Manual RTSS test
```bat
Test-RTSS-Profile.cmd Game.exe 60
```

## 🧹 Uninstallation
Run `Uninstall-FrameShift.cmd` and remove `%AppData%\Playnite\Extensions\FrameShift`.

## 📁 Repository structure
```text
FrameShift-for-Playnite/
├─ FrameShift.sln
├─ FrameShift.csproj
├─ README.md
├─ INSTALL.md
├─ LICENSE
├─ .gitignore
└─ FrameShiftPlugin/
   ├─ Source/
   ├─ RTSSBridge/
   ├─ extension.yaml
   ├─ build.cmd
   ├─ Install-FrameShift.cmd
   ├─ Uninstall-FrameShift.cmd
   └─ Test-RTSS-Profile.cmd
```

**No Toggle Fullscreen theme is included.** FrameShift is a standalone Playnite extension.

## 🖥️ Development / test environment
- Playnite 10.60
- Windows 11
- AOC CQ27G2U/BK
- 2560 × 1440
- up to 144 Hz
- NVIDIA GeForce RTX 5060 Ti 16 GB
- Intel Core i5-12400
- 32 GB DDR4

These details document the development environment; runtime operation is not intended to depend on this specific hardware.

## 📝 Releases
Recommended release naming:
```text
FrameShift v1.0.x
```

Use the GitHub Release for compiled ZIP assets and keep source code on `main`.

## 📄 License
All rights reserved. See `LICENSE`.