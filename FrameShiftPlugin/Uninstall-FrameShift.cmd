@echo off
setlocal
chcp 65001 >nul

net session >nul 2>&1
if errorlevel 1 (
  echo ERROR: This script must be run as Administrator.
  pause
  exit /b 1
)

echo Removing the FrameShift bridge and its scheduled task...
"%~dp0FrameShiftBridge.exe" uninstall
pause
