# FrameShift — Installation

## Requirements

- Windows (64-bit)
- Playnite 10
- RivaTuner Statistics Server (provides `RTSSHooks64.dll`)
- A Windows account with administrator rights (needed once, for the bridge)

## Install the plugin

1. Open `FrameShift_<version>.pext` with Playnite and confirm the installation.
2. Restart Playnite.
3. Confirm the FrameShift prompt to install the RTSS bridge and accept the UAC prompt.

The bridge is installed to `%ProgramFiles%\FrameShift\FrameShiftBridge.exe` and started through the `FrameShift RTSS Bridge` scheduled task. If the bridge is missing or outdated, FrameShift asks again the next time you change an FPS limit.

Repair: **Main menu → Extensions → FrameShift → Reinstall RTSS bridge**.

## Configure FPS

Open a game and choose **FrameShift**. Available profiles:

- 30 FPS
- 40 FPS
- 60 FPS
- 120 FPS
- OFF

The selected value is written to the RTSS profile for the game's executable.

## Launcher games

If Playnite cannot determine the final executable, start the game once. FrameShift can learn the actual process and save its executable name for future launches.

## Uninstall

1. **Main menu → Extensions → FrameShift → Remove RTSS bridge** (removes the scheduled task and `%ProgramFiles%\FrameShift`).
2. Uninstall the add-on in Playnite.

## Troubleshooting

Test an RTSS profile manually from an Administrator command prompt in the extension folder:

```bat
Test-RTSS-Profile.cmd Game.exe 60
```

`Install-FrameShift.cmd` / `Uninstall-FrameShift.cmd` (run as Administrator) do the same as the menu items.
