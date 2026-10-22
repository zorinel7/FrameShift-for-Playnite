@echo off
setlocal
chcp 65001 >nul
if "%~1"=="" (
  echo Użycie: Test-RTSS-Profile.cmd App.exe 60
  echo Przykład: Test-RTSS-Profile.cmd Cyberpunk2077.exe 60
  exit /b 1
)
if "%~2"=="" (
  echo Podaj limit: 0 (WYŁ.) albo od 10 do 500.
  exit /b 1
)
FrameShiftBridge.exe setprofile "%~1" %~2
pause
