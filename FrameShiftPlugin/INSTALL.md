# FrameShift — Plugin Installation

1. Close Playnite.
2. Build the project with `build.cmd`.
3. Copy `FrameShift.dll` and `FrameShiftBridge.exe` from `bin\Release` to the FrameShift extension directory.
4. Copy `extension.yaml`.
5. Run `Install-FrameShift.cmd` once as Administrator if the scheduled bridge task does not exist.
6. Start Playnite.

FrameShift v13 stores the selected limit in the RTSS profile belonging to the game's executable. A custom Toggle theme is not included or required.
