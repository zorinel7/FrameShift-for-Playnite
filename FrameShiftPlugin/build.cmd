@echo off
setlocal EnableExtensions
cd /d "%~dp0"
chcp 65001 >nul

echo ========================================
echo        FrameShift - BUILD
echo ========================================
echo.

set "MSBUILD="
if exist "%ProgramFiles%\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
if not defined MSBUILD if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
if not defined MSBUILD if exist "%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
if not defined MSBUILD if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" (
  for /f "usebackq delims=" %%P in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do if not defined MSBUILD set "MSBUILD=%%P"
)

if not defined MSBUILD (
  echo ERROR: MSBuild not found.
  pause
  exit /b 1
)

echo Using: %MSBUILD%
echo.

echo [1/2] Building RTSS bridge...
"%MSBUILD%" RTSSBridge\FrameShiftBridge.csproj /t:Build /p:Configuration=Release /p:Platform=x64 /m
if errorlevel 1 goto :fail

echo.
echo [2/2] Building Playnite plugin...
"%MSBUILD%" ..\FrameShift.csproj /t:Build /p:Configuration=Release /p:Platform=AnyCPU /m
if errorlevel 1 goto :fail

if not exist "..\bin\Release\FrameShift.dll" goto :fail
if not exist "RTSSBridge\bin\Release\FrameShiftBridge.exe" goto :fail

copy /Y "RTSSBridge\bin\Release\FrameShiftBridge.exe" "..\bin\Release\FrameShiftBridge.exe" >nul || goto :fail
copy /Y "extension.yaml" "..\bin\Release\extension.yaml" >nul || goto :fail
copy /Y "INSTALL.md" "..\bin\Release\INSTALL.md" >nul || goto :fail
copy /Y "Install-FrameShift.cmd" "..\bin\Release\Install-FrameShift.cmd" >nul || goto :fail
copy /Y "Uninstall-FrameShift.cmd" "..\bin\Release\Uninstall-FrameShift.cmd" >nul || goto :fail

echo.
echo ========================================
echo BUILD COMPLETED
echo ========================================
echo Output: ..\bin\Release
pause
exit /b 0

:fail
echo.
echo ========================================
echo BUILD FAILED
echo ========================================
pause
exit /b 1
