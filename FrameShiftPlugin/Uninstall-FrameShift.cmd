@echo off
setlocal
chcp 65001 >nul
set "TASK=FrameShift RTSS Bridge"
echo Removing scheduled task: %TASK%
schtasks /Delete /TN "%TASK%" /F
pause
