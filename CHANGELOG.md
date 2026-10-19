# Changelog

## Unreleased

### Fixed
- `FrameShiftBridge` did not compile: `Sanitize` contained raw CR/LF characters inside string literals instead of `\r` / `\n`.
- `Install-FrameShift.cmd` passed a malformed `/TR` value to `schtasks` (missing closing quote) and used the non-existent `/SC ONDEMAND` schedule type. It now uses `/SC ONLOGON`, as the released 1.0.8 package does, and checks for Administrator rights up front.
- `FrameShift.csproj`: the `AfterBuild` target was defined before the `Microsoft.CSharp.targets` import and was silently overridden, so `extension.yaml` and `INSTALL.md` were never copied to the output.

### Build
- `Playnite.SDK.dll` is resolved from `/p:PlayniteSdkPath`, the repository root or the local Playnite installation, with a clear error when it is missing.
- `build.cmd /nopause` for non-interactive builds; `Test-RTSS-Profile.cmd` is now copied to the output.
