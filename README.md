# FrameShift for Playnite

**Per-game FPS limiter for Playnite using RivaTuner Statistics Server (RTSS).**

> ## ⚠️ TOGGLE THEME REQUIRED FOR FULLSCREEN
> To use FrameShift in **Playnite Fullscreen**, you must install **Toggle - Zorinel / FrameShift Fork**.
>
> Theme repository: https://github.com/zorinel7/Toggle-Zorinel-FrameShift
>
> Toggle provides the FrameShift button in the game details view. **FrameShift and the Toggle theme must be installed together** for the Fullscreen integration to work.

## 🎮 Features

- **30 / 40 / 60 / 120 FPS / OFF**
- Per-game FPS limits
- RTSS profile assigned to the correct game executable
- Automatic game executable detection
- Remembers the selected FPS mode for each game
- Supports games launched through **Steam, EA app, Epic Games, Ubisoft Connect** and other launchers
- Ignores launcher processes when selecting the final game profile
- Dedicated **FrameShift RTSS Bridge**
- Controller-friendly Fullscreen integration
- Automatic localization based on the language selected in Playnite

## 📦 Installation

### FrameShift plugin

The GitHub Release provides the plugin as a **ZIP package**. Extract it manually because this release is not distributed as a `.pext` package.

1. Close Playnite.
2. Download the latest FrameShift ZIP from **Releases**.
3. Extract the contents into:

```
%AppData%\Playnite\Extensions\FrameShift\
```

The folder should contain:

```
FrameShift.dll
FrameShiftBridge.exe
extension.yaml
```

Do not leave an extra nested folder.

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

FrameShift follows the language selected in **Playnite**. The FrameShift interface and its Toggle integration use the corresponding Playnite language automatically.

Available localization codes:

**af_ZA, ar_SA, bg_BG, ca_ES, cs_CZ, cy_GB, da_DK, de_DE, el_GR, en_US, eo_UY, es_ES, et_EE, fa_IR, fi_FI, fr_FR, ga_IE, gl_ES, he_IL, hr_HR, hu_HU, id_ID, it_IT, ja_JP, ko_KR, lt_LT, mr_IN, nl_NL, no_NO, pl_PL, pt_BR, pt_PT, ro_RO, ru_RU, si_LK, sk_SK, sl_SI, sr_SP, sv_SE, tr_TR, uk_UA, vi_VN, zh_CN, zh_TW.**

## 🔌 RTSS Bridge

FrameShift uses `FrameShiftBridge.exe` to communicate with RTSS and update the selected application's FPS limit.

## 🧩 Toggle - Zorinel Fork

**Original theme:** Toggle  
**Fork:** Zorinel  
**Repository:** https://github.com/zorinel7/Toggle-Zorinel-FrameShift

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

Current release: **FrameShift v1.0.8**
