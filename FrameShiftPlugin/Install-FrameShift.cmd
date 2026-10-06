@echo off
setlocal EnableExtensions
chcp 65001 >nul

echo ========================================
echo       FrameShift - INSTALL BRIDGE
echo ========================================
echo.

set "BRIDGE=%~dp0FrameShiftBridge.exe"
if not exist "%BRIDGE%" (
  echo ERROR: FrameShiftBridge.exe was not found.
  echo Build the Release configuration first.
  pause
  exit /b 1
)

set "TASK=FrameShift RTSS Bridge"
echo Creating scheduled task: %TASK%
schtasks /Create /TN "%TASK%" /TR "\"%BRIDGE%" worker" /SC ONDEMAND /RL HIGHEST /F
if errorlevel 1 (
  echo.
  echo ERROR: Could not create the scheduled task.
  echo Run this script as Administrator.
  pause
  exit /b 1
)

echo.
echo FrameShift bridge installed successfully.
echo.
pause
exit /b 0
