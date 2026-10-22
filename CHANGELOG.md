# Changelog

## 1.2.0 (2026-10-22)

### Added
- **Customizable FPS modes.** Desktop Mode: **Main menu → Extensions → FrameShift → Customize FPS modes…** sets the four modes (10–500 FPS, all different; **Restore defaults** = 30 / 40 / 60 / 120). OFF is always available.
- The modes are used in every FPS menu: the game menu in Desktop and in Fullscreen with any theme, and the Toggle theme's FrameShift button. Editing is Desktop-only; the menu item is hidden in Fullscreen.
- A game whose limit is no longer one of the modes keeps it, and the menus still show it with ✓.
- New texts translated into all 44 languages.

### Changed
- The RTSS bridge accepts 0 (OFF) or 10–500 FPS instead of the fixed 30 / 40 / 60 / 120 list. The bridge changes, so FrameShift asks once (UAC) to refresh it after the update.
- Existing settings get the default modes 30 / 40 / 60 / 120; per-game limits are unchanged.

## 1.1.1 (2026-10-19, package updated 2026-10-20)

### Added
- FPS limit can be set from the game menu in **any** Fullscreen theme: **⋯ → Extensions → FrameShift → 30 / 40 / 60 / 120 FPS / OFF** (the active mode is marked with ✓). The Toggle theme is now optional (it still adds the FrameShift button on the game details page).
- Desktop mode uses the same **FrameShift** submenu in the game context menu instead of a separate window.

### Changed
- Error messages use Playnite's own dialogs, so they work with a controller in Fullscreen.

### Fixed
- Xbox app / Microsoft Store (GDK) games: FrameShift saved `gamelaunchhelper.exe` (the same launcher stub in every Xbox game) instead of the game EXE, so the limit did not reach the game and was shared between all Xbox games. The real executable is now read from `MicrosoftGame.config`; anti-cheat bootstrappers (`start_protected_game.exe`) fall back to the folder scan, which now also recognises `Binaries\WinGDK`.
- Launcher stubs are no longer accepted as RTSS profiles: `gamelaunchhelper.exe`, `start_protected_game.exe`, `mcclauncher.exe` (Halo MCC → `MCC-Win64-Shipping.exe`), `PlayRDR2.exe` (→ `RDR2.exe`), `launcher.exe`; executables with "launcher" in the name are ranked below the game (Skate → `Skate.exe`).
- Games that already saved one of these launchers are re-resolved on the next launch, and the stored FPS limit is written to the new EXE's RTSS profile before the game starts.
- Learning the EXE from the started process works for 64-bit games (Playnite is 32-bit, `Process.MainModule` failed).

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
