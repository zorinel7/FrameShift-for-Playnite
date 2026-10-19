@echo off
setlocal EnableExtensions
chcp 65001 >nul

rem Optional: Playnite installs the bridge automatically on first start (one UAC prompt).
rem Use this script only for manual ZIP installs or to repair the bridge.

echo ========================================
echo       FrameShift - INSTALL BRIDGE
echo ========================================
echo.

net session >nul 2>&1
if errorlevel 1 (
  echo ERROR: This script must be run as Administrator.
  echo Right-click Install-FrameShift.cmd and choose "Run as administrator".
  pause
  exit /b 1
)

if not exist "%~dp0FrameShiftBridge.exe" (
  echo ERROR: FrameShiftBridge.exe was not found next to this script.
  pause
  exit /b 1
)

"%~dp0FrameShiftBridge.exe" install
if errorlevel 1 (
  echo.
  echo ERROR: Bridge installation failed.
  pause
  exit /b 1
)

echo.
echo FrameShift bridge installed to "%ProgramFiles%\FrameShift".
echo.
pause
exit /b 0
