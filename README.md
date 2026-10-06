# FrameShift for Playnite

**Kontrolerowy menedżer limitów FPS dla gier w Playnite, wykorzystujący RivaTuner Statistics Server (RTSS).**

> ## ⚠️ WYMAGANY MOTYW TOGGLE
> Aby korzystać z FrameShift w **Playnite Fullscreen**, potrzebny jest przygotowany przeze mnie motyw **Toggle – Zorinel / FrameShift Fork**.
>
> Motyw Toggle odpowiada za wyświetlanie przycisku FrameShift w widoku szczegółów gry. **FrameShift i motyw Toggle muszą być zainstalowane razem**, jeżeli chcesz korzystać z funkcji w Fullscreen.
>
> Motyw zachowuje oryginalny wygląd i działanie Toggle, a integracja została dostosowana do FrameShift.

## 🎮 Funkcje

- **30 / 40 / 60 / 120 FPS / OFF**
- osobny limit FPS dla każdej gry
- profil RTSS przypisany do właściwego pliku EXE
- automatyczne wykrywanie EXE gry
- zapamiętywanie ustawienia FPS dla każdej gry
- obsługa gier uruchamianych przez Steam, EA app, Epic Games, Ubisoft Connect i inne launchery
- automatyczne pomijanie procesów launcherów jako docelowego profilu
- FrameShift RTSS Bridge
- obsługa języka wybranego w Playnite

## 🌍 Lokalizacja

FrameShift korzysta z języka ustawionego w Playnite i automatycznie zmienia teksty dodatku po zmianie języka aplikacji.

Dostępne są lokalizacje:
**af_ZA, ar_SA, bg_BG, ca_ES, cs_CZ, cy_GB, da_DK, de_DE, el_GR, en_US, eo_UY, es_ES, et_EE, fa_IR, fi_FI, fr_FR, ga_IE, gl_ES, he_IL, hr_HR, hu_HU, id_ID, it_IT, ja_JP, ko_KR, lt_LT, mr_IN, nl_NL, no_NO, pl_PL, pt_BR, pt_PT, ro_RO, ru_RU, si_LK, sk_SK, sl_SI, sr_SP, sv_SE, tr_TR, uk_UA, vi_VN, zh_CN, zh_TW.**

Lokalizacja obejmuje również teksty FrameShift używane w integracji z motywem Toggle.

## 🔧 Jak działa

```text
Playnite
   ↓
FrameShift
   ↓
wykryty EXE gry
   ↓
profil aplikacji RTSS
   ↓
FramerateLimit
```

Przykład:

```text
Gra A → 60 FPS
Gra B → 40 FPS
Gra C → 120 FPS
```

Przy grach launcherowych FrameShift może najpierw uruchomić grę, a następnie wykorzystać rzeczywisty proces gry jako docelowy profil RTSS.

## 📋 Profile

| Tryb | Zastosowanie |
|---|---|
| **30 FPS** | niski pobór mocy / spokojne gry |
| **40 FPS** | płynny tryb konsolowy |
| **60 FPS** | standardowy limit |
| **120 FPS** | monitory wysokiego odświeżania |
| **OFF** | wyłączenie limitu |

## 🛠️ Wymagania

- Windows 10/11
- Playnite **10.60**
- RivaTuner Statistics Server (RTSS)
- `RTSSHooks64.dll`
- **Motyw Toggle – Zorinel / FrameShift Fork** do działania w Playnite Fullscreen

## 📦 Instalacja

1. Zainstaluj **RTSS**.
2. Zainstaluj **FrameShift** z pliku Release.
3. Zainstaluj przygotowany **motyw Toggle – Zorinel / FrameShift Fork**.
4. Uruchom ponownie Playnite.
5. Przejdź do **Fullscreen** i otwórz szczegóły gry.
6. Wybierz **FrameShift** i ustaw limit FPS.

## 🔌 RTSS Bridge

FrameShift wykorzystuje dedykowany `FrameShiftBridge.exe` do komunikacji z RTSS i ustawiania limitu dla konkretnego procesu/aplikacji.

## 🧪 Test RTSS

```bat
Test-RTSS-Profile.cmd Game.exe 60
```

## 🖥️ Środowisko testowe

FrameShift był testowany na:

- **Playnite 10.60**
- **Windows 11**
- **Intel Core i5-12400**
- **NVIDIA GeForce RTX 5060 Ti 16 GB**
- **32 GB DDR4**
- **AOC CQ27G2U/BK**
- **2560 × 1440**
- do **144 Hz**

## 🧩 Toggle – Zorinel Fork

FrameShift korzysta z przygotowanej przeze mnie wersji motywu Toggle.

**Oryginalny motyw:** Toggle  
**Fork:** Zorinel  
**Cel forka:** integracja z FrameShift oraz lokalizacja interfejsu FrameShift bez zmiany wyglądu i podstawowego działania motywu.

## 📥 Releases

Gotowe, skompilowane wersje FrameShift są publikowane w **GitHub Releases**.

Najnowsza wersja:
**FrameShift v1.0.8**

