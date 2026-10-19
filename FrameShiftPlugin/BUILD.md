# FrameShift — Build

## Development environment

- Visual Studio 2022
- .NET Framework 4.6.2 Developer Pack
- .NET desktop development workload
- Playnite SDK reference (`Playnite.SDK.dll`)

## Playnite SDK

`Playnite.SDK.dll` is not stored in the repository. The build looks for it in this order:

1. `/p:PlayniteSdkPath=<path to Playnite.SDK.dll>`
2. `Playnite.SDK.dll` in the repository root (ignored by git)
3. `%LocalAppData%\Playnite\Playnite.SDK.dll` (installed Playnite)

If none is found, the build stops with an explanatory error.

## Build

Open `FrameShift.sln` in Visual Studio and build **Release**.

Or run from a Developer Command Prompt:

```bat
FrameShiftPlugin\build.cmd
```

Use `build.cmd /nopause` for CI or scripted builds.

## Projects

- `FrameShift.csproj` — Playnite plugin
- `FrameShiftPlugin/RTSSBridge/FrameShiftBridge.csproj` — RTSS bridge

## Output

The build produces `FrameShift.dll` and `FrameShiftBridge.exe` in `bin\Release`, and `build.cmd` packs them into `bin\FrameShift_<version>.pext` (version taken from `extension.yaml`).

The plugin targets **.NET Framework 4.6.2** and uses WPF.
