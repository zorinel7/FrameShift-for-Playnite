@echo off
setlocal EnableExtensions
chcp 65001 >nul

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

set "BRIDGE=%~dp0FrameShiftBridge.exe"
if not exist "%BRIDGE%" (
  echo ERROR: FrameShiftBridge.exe was not found:
  echo %BRIDGE%
  pause
  exit /b 1
)

set "TASK=FrameShift RTSS Bridge"
echo Creating scheduled task: %TASK%
rem The plugin starts the task on demand with "schtasks /Run". schtasks has no
rem on-demand schedule type, so ONLOGON is used; at logon the worker finds no
rem request file and exits immediately.
schtasks /Create /TN "%TASK%" /TR "\"%BRIDGE%\" worker" /SC ONLOGON /RL HIGHEST /F
if errorlevel 1 (
  echo.
  echo ERROR: Could not create the scheduled task.
  pause
  exit /b 1
)

echo.
echo FrameShift bridge installed successfully.
echo.
pause
exit /b 0
