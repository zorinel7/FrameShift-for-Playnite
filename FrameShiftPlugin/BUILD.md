# FrameShift — Build

## Development environment

- Visual Studio 2022
- .NET Framework 4.6.2 Developer Pack
- .NET desktop development workload
- Playnite SDK reference

## Build

Open `FrameShift.sln` in Visual Studio and build **Release**.

Or run from a Developer Command Prompt:

```bat
FrameShiftPlugin\build.cmd
```

## Projects

- `FrameShift.csproj` — Playnite plugin
- `FrameShiftPlugin/RTSSBridge/FrameShiftBridge.csproj` — RTSS bridge

## Output

The build produces `FrameShift.dll` and `FrameShiftBridge.exe`.

The plugin targets **.NET Framework 4.6.2** and uses WPF.
