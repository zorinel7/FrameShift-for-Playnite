using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading;

namespace RTSSBridge
{
    /// <summary>
    /// Installs the bridge into %ProgramFiles%\FrameShift and registers the elevated scheduled task.
    /// The task must never point at the Playnite extension folder: that folder is writable by the
    /// user, so a replaced EXE would run with administrator rights.
    /// </summary>
    internal static class Installer
    {
        internal const string TaskName = "FrameShift RTSS Bridge";
        private const string ExeName = "FrameShiftBridge.exe";

        private static string InstallDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "FrameShift");

        internal static int Install()
        {
            if (!IsElevated()) { Console.WriteLine("ERROR|not_elevated"); return 4; }

            string source = Process.GetCurrentProcess().MainModule.FileName;
            string target = Path.Combine(InstallDir, ExeName);
            Directory.CreateDirectory(InstallDir);

            if (!string.Equals(Path.GetFullPath(source), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
                Retry(() => File.Copy(source, target, true));

            string args = "/Create /TN \"" + TaskName + "\" /TR \"\\\"" + target + "\\\" worker\" /SC ONLOGON /RL HIGHEST /F";
            string output;
            int exit = RunSchtasks(args, out output);
            if (exit != 0) { Console.WriteLine("ERROR|schtasks_create=" + exit + "|" + Clean(output)); return 5; }

            Console.WriteLine("OK|installed=" + target);
            return 0;
        }

        internal static int Uninstall()
        {
            if (!IsElevated()) { Console.WriteLine("ERROR|not_elevated"); return 4; }

            string output;
            RunSchtasks("/Delete /TN \"" + TaskName + "\" /F", out output);

            try
            {
                if (Directory.Exists(InstallDir)) Retry(() => Directory.Delete(InstallDir, true));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR|delete_failed=" + Clean(ex.Message));
                return 5;
            }

            Console.WriteLine("OK|uninstalled");
            return 0;
        }

        private static bool IsElevated()
        {
            using (var identity = WindowsIdentity.GetCurrent())
                return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }

        // A worker started by the task may still be running for a moment and lock the EXE.
        private static void Retry(Action action)
        {
            for (int attempt = 1; ; attempt++)
            {
                try { action(); return; }
                catch (IOException) when (attempt < 10) { Thread.Sleep(300); }
                catch (UnauthorizedAccessException) when (attempt < 10) { Thread.Sleep(300); }
            }
        }

        private static int RunSchtasks(string arguments, out string output)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var process = Process.Start(psi))
            {
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                output = (stdout + " " + stderr).Trim();
                return process.ExitCode;
            }
        }

        private static string Clean(string value)
        {
            return (value ?? "").Replace("\r", " ").Replace("\n", " ").Replace("|", "/").Trim();
        }
    }
}
