# Changelog

## 1.1.0 (2026-10-19)

### Changed
- Author set to zorinel; added an add-on icon (`icon.png`).
- Localization restored in the source tree: 44 languages in `FrameShiftPlugin/Localization` (strings recovered from the 1.0.8 release), plus translations for the new bridge messages and menu items. The language follows Playnite; unknown regions fall back to the same language (e.g. `es_MX` → `es_ES`), then to English. Arabic, Persian and Hebrew use right-to-left layout.
- FrameShift is distributed as a single `.pext`. The RTSS bridge no longer has to be installed manually: on the first Playnite start after installing or updating the add-on, FrameShift asks once and installs the bridge (one UAC prompt).
- New main menu items: **Extensions → FrameShift → Reinstall RTSS bridge / Remove RTSS bridge**.
- `Install-FrameShift.cmd` / `Uninstall-FrameShift.cmd` are now thin wrappers around `FrameShiftBridge.exe install|uninstall`.

### Security
- The elevated scheduled task now runs `%ProgramFiles%\FrameShift\FrameShiftBridge.exe` instead of the copy in the Playnite extension folder, which is writable by the user. Previously, replacing that EXE gave administrator rights without a UAC prompt.
- The bridge validates the request ID and the profile name (must be a plain `*.exe` file name) before calling RTSS. The worker no longer accepts the global profile.

### Fixed
- Fullscreen FPS menu: the controller **B** (cancel) button now closes the menu. Playnite sends controller input as `GameControllerInputEventArgs` raised directly as a bubbling `KeyDown` on the focused element (no `PreviewKeyDown`, no Escape key); the menu now listens on `KeyDown` and checks Playnite's `CancellationBinding`.
- Fullscreen FPS menu: the window sizes itself to its content, so the last option (OFF) and its focus ring are no longer cut off.
- `FrameShiftBridge` did not compile: `Sanitize` contained raw CR/LF characters inside string literals instead of `\r` / `\n`.
- `Install-FrameShift.cmd` passed a malformed `/TR` value to `schtasks` (missing closing quote) and used the non-existent `/SC ONDEMAND` schedule type. It now uses `/SC ONLOGON`, as the released 1.0.8 package does, and checks for Administrator rights up front.
- `FrameShift.csproj`: the `AfterBuild` target was defined before the `Microsoft.CSharp.targets` import and was silently overridden, so `extension.yaml` and `INSTALL.md` were never copied to the output.

### Build
- `Playnite.SDK.dll` is resolved from `/p:PlayniteSdkPath`, the repository root or the local Playnite installation, with a clear error when it is missing.
- `build.cmd /nopause` for non-interactive builds; `Test-RTSS-Profile.cmd` is now copied to the output.
