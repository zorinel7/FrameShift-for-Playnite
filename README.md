# FrameShift for Playnite

**Per-game FPS limiter for Playnite using RivaTuner Statistics Server (RTSS).**

> ## ⚠️ TOGGLE THEME REQUIRED FOR FULLSCREEN
> To use FrameShift in **Playnite Fullscreen**, you must install **Toggle - Zorinel / FrameShift Fork**.
>
> Theme repository: https://github.com/zorinel7/Toggle-Zorinel-FrameShift
>
> The original Toggle theme was created by **Jono (jonosellier)**. This repository uses a Zorinel fork of Toggle for FrameShift integration. Original project: https://github.com/jonosellier/toggle-theme-playnite
>
> Toggle provides the FrameShift button in the game details view. **FrameShift and the Toggle theme must be installed together** for the Fullscreen integration to work.

## 🎮 Features

- **30 / 40 / 60 / 120 FPS / OFF**
- Per-game FPS limits
- RTSS profile assigned to the correct game executable
- Automatic game executable detection
- Remembers the selected FPS mode for each game
- Supports games launched through **Steam, EA app, Epic Games, Ubisoft Connect, the Xbox app (Game Pass / Microsoft Store)** and other launchers
- Ignores launcher processes and stubs (e.g. Xbox `gamelaunchhelper.exe`, `PlayRDR2.exe`, `mcclauncher.exe`) when selecting the final game profile; Xbox games use the executable listed in `MicrosoftGame.config`
- Dedicated **FrameShift RTSS Bridge**, installed automatically on first start (one UAC prompt, no scripts)
- Controller-friendly Fullscreen integration (A selects, B closes the menu)
- Automatic localization based on the language selected in Playnite (44 languages)

## 📦 Installation

### FrameShift plugin

1. Download the latest `FrameShift_<version>.pext` from **Releases**.
2. Open the `.pext` file with Playnite (double-click or drag it onto Playnite) and confirm the installation.
3. Restart Playnite when asked.
4. On the first start FrameShift asks to install its **RTSS bridge**. Confirm, then accept the Windows administrator (UAC) prompt.

That's it — no scripts to run. The bridge is copied to `%ProgramFiles%\FrameShift\` and registered as the `FrameShift RTSS Bridge` scheduled task, so later FPS changes do not show UAC prompts. After a FrameShift update the bridge is refreshed the same way (one UAC prompt).

Administrator rights are needed once because RTSS stores its profiles under `Program Files`.

**Removing FrameShift:** before uninstalling the add-on, use **Main menu → Extensions → FrameShift → Remove RTSS bridge**. Playnite only deletes the add-on folder, not the scheduled task.

<details>
<summary>Manual ZIP installation (advanced)</summary>

Extract the package into `%AppData%\Playnite\Extensions\FrameShift\` (no nested folder) and either start Playnite (the bridge is installed on first start) or run `Install-FrameShift.cmd` as Administrator.
</details>

### Toggle Fullscreen theme

The Toggle theme is available as a **Playnite `.pthm` package**.

1. Download **Toggle - Zorinel / FrameShift Fork**:
   https://github.com/zorinel7/Toggle-Zorinel-FrameShift
2. Download the latest `.pthm` file from its Releases.
3. Open the `.pthm` file with Playnite to install the theme.
4. Start/restart Playnite.
5. Open **Fullscreen Mode -> Settings -> Visuals -> Theme**.
6. Select **Toggle**.
7. Open a game's details page. The **FrameShift** button should now be visible.

A ZIP/manual installation is also available in the Toggle repository if required.

> **Important:** FrameShift alone does not add the visible Fullscreen button. The button is provided by the Toggle theme integration.

## 🖥️ Desktop Mode

FrameShift also works in **Playnite Desktop Mode** and does not require the Toggle theme there.

In Desktop Mode, select a single game and open its **game context menu**. Choose **FrameShift** to open the FPS selector. You can then choose **30, 40, 60, 120 FPS or OFF** for that game.

The selected FPS limit is stored for the game and applied to its RTSS profile. The same per-game settings are used in both Desktop Mode and Fullscreen Mode.

## 🔧 How it works

```
Playnite
   ↓
FrameShift
   ↓
Game executable detection
   ↓
RTSS application profile
   ↓
FramerateLimit
```

For launcher-based games, FrameShift can use the actual game process started by Playnite instead of the launcher process.

## 📋 FPS Profiles

| Mode | Use |
|---|---|
| **30 FPS** | Low-power / cinematic |
| **40 FPS** | Console-style balanced mode |
| **60 FPS** | Standard gaming |
| **120 FPS** | High-refresh gaming |
| **OFF** | Disable the FPS limit |

## 🌍 Localization

FrameShift follows the language selected in **Playnite** (read when Playnite starts). Unknown regional variants fall back to the same language (for example `es_MX` → `es_ES`), otherwise to English. Arabic, Persian and Hebrew use a right-to-left layout.

Available localization codes:

**af_ZA, ar_SA, bg_BG, ca_ES, cs_CZ, cy_GB, da_DK, de_DE, el_GR, en_US, eo_UY, es_ES, et_EE, fa_IR, fi_FI, fr_FR, ga_IE, gl_ES, he_IL, hr_HR, hu_HU, id_ID, it_IT, ja_JP, ko_KR, lt_LT, mr_IN, nl_NL, no_NO, pl_PL, pt_BR, pt_PT, ro_RO, ru_RU, si_LK, sk_SK, sl_SI, sr_SP, sv_SE, tr_TR, uk_UA, vi_VN, zh_CN, zh_TW.**

## 🔌 RTSS Bridge

FrameShift uses `FrameShiftBridge.exe` (64-bit) to communicate with RTSS and update the selected application's FPS limit. Writing RTSS profiles needs administrator rights, so the bridge runs from `%ProgramFiles%\FrameShift\` through the `FrameShift RTSS Bridge` scheduled task. The plugin installs and updates it automatically; **Main menu → Extensions → FrameShift** offers **Reinstall RTSS bridge** and **Remove RTSS bridge**.

## 🧩 Toggle - Zorinel Fork

**Original theme:** Toggle  
**Original creator:** Jono (jonosellier)  
**Original repository:** https://github.com/jonosellier/toggle-theme-playnite  
**Fork:** Zorinel  
**Fork repository:** https://github.com/zorinel7/Toggle-Zorinel-FrameShift

The fork preserves the original Toggle appearance and core behavior while adding FrameShift integration and localization support.

## 🖥️ Tested Environment

- **Playnite 10.60**
- **Windows 11**
- **Intel Core i5-12400**
- **NVIDIA GeForce RTX 5060 Ti 16 GB**
- **32 GB DDR4**
- **AOC CQ27G2U/BK**
- **2560 x 1440**
- Up to **144 Hz**

## 📥 Releases

Compiled builds are distributed through GitHub Releases.

Current release: **FrameShift v1.1.1** — see [CHANGELOG.md](CHANGELOG.md).
