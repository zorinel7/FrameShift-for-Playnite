using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace FrameShift
{
    /// <summary>
    /// Keeps the elevated bridge installed. The .pext ships FrameShiftBridge.exe in the extension
    /// folder; "FrameShiftBridge.exe install" (run once with UAC) copies it to %ProgramFiles%\FrameShift
    /// and registers the scheduled task that the plugin starts for every RTSS change.
    /// </summary>
    internal static class BridgeSetup
    {
        internal const string TaskName = "FrameShift RTSS Bridge";
        private const string ExeName = "FrameShiftBridge.exe";
        private const int ErrorCancelled = 1223;

        internal static string PackagedBridgePath =>
            Path.Combine(Path.GetDirectoryName(typeof(BridgeSetup).Assembly.Location), ExeName);

        // Playnite is a 32-bit process, so SpecialFolder.ProgramFiles would return "Program Files (x86)".
        internal static string InstalledBridgePath
        {
            get
            {
                string programFiles = Environment.GetEnvironmentVariable("ProgramW6432");
                if (string.IsNullOrWhiteSpace(programFiles))
                    programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                return Path.Combine(programFiles, "FrameShift", ExeName);
            }
        }

        /// <summary>True when the installed bridge matches the packaged one and the task points at it.</summary>
        internal static bool IsUpToDate()
        {
            string packaged = PackagedBridgePath, installed = InstalledBridgePath;
            if (!File.Exists(packaged) || !File.Exists(installed)) return false;
            if (!FileHash(packaged).SequenceEqual(FileHash(installed))) return false;

            string taskXml;
            return RunHidden("schtasks.exe", "/Query /TN \"" + TaskName + "\" /XML", out taskXml) == 0 &&
                   taskXml.IndexOf(installed, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Runs the packaged bridge elevated. Returns null on success, otherwise an error text.</summary>
        internal static string RunElevated(string command)
        {
            string packaged = PackagedBridgePath;
            if (!File.Exists(packaged)) return Loc.Get("NoBridge");
            try
            {
                var psi = new ProcessStartInfo(packaged, command)
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (var process = Process.Start(psi))
                {
                    if (!process.WaitForExit(120000)) return Loc.Format("BridgeStepFailed", "timeout");
                    return process.ExitCode == 0 ? null : Loc.Format("BridgeStepFailed", process.ExitCode);
                }
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == ErrorCancelled)
            {
                return Loc.Get("BridgeUacDenied");
            }
        }

        private static byte[] FileHash(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
                return sha.ComputeHash(stream);
        }

        private static int RunHidden(string fileName, string arguments, out string output)
        {
            var psi = new ProcessStartInfo(fileName, arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var process = Process.Start(psi))
            {
                output = process.StandardOutput.ReadToEnd();
                process.StandardError.ReadToEnd();
                process.WaitForExit();
                return process.ExitCode;
            }
        }
    }
}
