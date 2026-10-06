# FrameShift — Installation

## Requirements

- Windows
- Playnite 10.60
- RivaTuner Statistics Server
- `RTSSHooks64.dll`

## Install the plugin

1. Close Playnite.
2. Open the Playnite extensions directory: `%AppData%\Playnite\Extensions\FrameShift\`
3. Copy `FrameShift.dll`, `FrameShiftBridge.exe` and `extension.yaml` into it.
4. Run `Install-FrameShift.cmd` as Administrator.
5. Start Playnite again.

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

## Troubleshooting

Test an RTSS profile manually:

```bat
Test-RTSS-Profile.cmd Game.exe 60
```

If the bridge task is missing, run `Install-FrameShift.cmd` again as Administrator.

To remove the scheduled task, run `Uninstall-FrameShift.cmd`.
