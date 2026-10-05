using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Threading;

namespace RTSSBridge
{
    internal static class Program
    {
        private const uint LIMITER_DISABLED = 4;
        private const uint WM_RTSS_UPDATESETTINGS = 0x8000 + 100;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void LoadProfileDelegate([MarshalAs(UnmanagedType.LPStr)] string profile);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void SaveProfileDelegate([MarshalAs(UnmanagedType.LPStr)] string profile);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate bool GetProfilePropertyDelegate([MarshalAs(UnmanagedType.LPStr)] string name, IntPtr data, uint size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate bool SetProfilePropertyDelegate([MarshalAs(UnmanagedType.LPStr)] string name, IntPtr data, uint size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void UpdateProfilesDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate uint SetFlagsDelegate(uint andMask, uint xorMask);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate uint GetFlagsDelegate();

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private static IntPtr module;
        private static LoadProfileDelegate loadProfile;
        private static SaveProfileDelegate saveProfile;
        private static GetProfilePropertyDelegate getProfileProperty;
        private static SetProfilePropertyDelegate setProfileProperty;
        private static UpdateProfilesDelegate updateProfiles;
        private static SetFlagsDelegate setFlags;
        private static GetFlagsDelegate getFlags;
        private static readonly object RtssLock = new object();

        private static int Main(string[] args)
        {
            try
            {
                if (args.Length == 1 && string.Equals(args[0], "worker", StringComparison.OrdinalIgnoreCase))
                    return RunWorker();

                if (args.Length == 3 && string.Equals(args[0], "setprofile", StringComparison.OrdinalIgnoreCase))
                {
                    int fps;
                    if (!int.TryParse(args[2], out fps) || !IsValidFps(fps) || string.IsNullOrWhiteSpace(args[1]))
                    {
                        Console.WriteLine("ERROR|usage=setprofile <App.exe> <0|30|40|60|120>");
                        return 2;
                    }
                    Result result = SetProfileFps(args[1], fps);
                    Console.WriteLine(result.Text);
                    return result.Ok ? 0 : 3;
                }

                if (args.Length == 2 && string.Equals(args[0], "getprofile", StringComparison.OrdinalIgnoreCase))
                {
                    Result result = GetProfileFps(args[1]);
                    Console.WriteLine(result.Text);
                    return result.Ok ? 0 : 3;
                }

                if (args.Length == 2 && string.Equals(args[0], "set", StringComparison.OrdinalIgnoreCase))
                {
                    int fps;
                    if (!int.TryParse(args[1], out fps) || !IsValidFps(fps))
                    {
                        Console.WriteLine("ERROR|invalid_fps=" + args[1]);
                        return 2;
                    }
                    Result result = SetProfileFps("", fps);
                    Console.WriteLine(result.Text);
                    return result.Ok ? 0 : 3;
                }

                Console.WriteLine("ERROR|usage=FrameShiftBridge.exe worker | setprofile <App.exe> <fps> | getprofile <App.exe>");
                return 2;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR|" + ex.GetType().Name + "=" + Sanitize(ex.Message));
                return 1;
            }
        }

        private static bool IsValidFps(int fps) => fps == 0 || fps == 30 || fps == 40 || fps == 60 || fps == 120;

        private static int RunWorker()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FrameShift");
            Directory.CreateDirectory(dir);
            string requestFile = Path.Combine(dir, "request.txt");
            string resultFile = Path.Combine(dir, "result.txt");

            try
            {
                if (!File.Exists(requestFile)) return 0;

                string request = File.ReadAllText(requestFile).Trim();
                try { File.Delete(requestFile); } catch { }

                string[] parts = request.Split(new[] { '|' }, 3);
                if (parts.Length != 3)
                {
                    WriteResult(resultFile, "unknown", "ERROR|invalid_request");
                    return 2;
                }

                int fps;
                if (!int.TryParse(parts[2], out fps) || !IsValidFps(fps))
                {
                    WriteResult(resultFile, parts[0], "ERROR|invalid_fps=" + Sanitize(parts[2]));
                    return 2;
                }

                Result result = SetProfileFps(parts[1], fps);
                WriteResult(resultFile, parts[0], result.Text);
                return result.Ok ? 0 : 3;
            }
            catch (Exception ex)
            {
                try { WriteResult(resultFile, "unknown", "ERROR|worker=" + ex.GetType().Name + "=" + Sanitize(ex.Message)); } catch { }
                return 1;
            }
        }

        private static void WriteResult(string resultFile, string id, string text)
        {
            string tmp = resultFile + ".tmp." + Process.GetCurrentProcess().Id;
            File.WriteAllText(tmp, id + "|" + text);
            if (File.Exists(resultFile)) File.Delete(resultFile);
            File.Move(tmp, resultFile);
        }

        private static Result SetProfileFps(string profile, int fps)
        {
            lock (RtssLock)
            {
                string rtssPath = FindRtssPath();
                if (string.IsNullOrWhiteSpace(rtssPath)) return Result.Fail("ERROR|rtss_not_found");

                string dllPath = Path.Combine(rtssPath, "RTSSHooks64.dll");
                if (!File.Exists(dllPath)) return Result.Fail("ERROR|missing_dll=" + Sanitize(dllPath));

                EnsureRtssRunning(rtssPath);
                LoadApi(dllPath);

                string profileForApi = profile ?? "";
                loadProfile(profileForApi);
                int before = GetIntProperty("FramerateLimit");

                SetIntProperty("FramerateLimitDenominator", 1);
                SetIntProperty("FramerateLimit", fps);
                saveProfile(profileForApi);
                updateProfiles();

                if (fps > 0) setFlags(~LIMITER_DISABLED, 0);
                PostRtssUpdateMessage();

                loadProfile(profileForApi);
                int after = GetIntProperty("FramerateLimit");
                bool ok = after == fps;

                string text = (ok ? "OK" : "ERROR") +
                    "|profile=" + Sanitize(profileForApi) +
                    "|before=" + before +
                    "|after=" + after +
                    "|fps=" + fps +
                    "|rtss=" + Sanitize(rtssPath);
                return ok ? Result.Success(text) : Result.Fail(text);
            }
        }

        private static Result GetProfileFps(string profile)
        {
            lock (RtssLock)
            {
                string rtssPath = FindRtssPath();
                if (string.IsNullOrWhiteSpace(rtssPath)) return Result.Fail("ERROR|rtss_not_found");

                string dllPath = Path.Combine(rtssPath, "RTSSHooks64.dll");
                if (!File.Exists(dllPath)) return Result.Fail("ERROR|missing_dll=" + Sanitize(dllPath));

                EnsureRtssRunning(rtssPath);
                LoadApi(dllPath);
                string profileForApi = profile ?? "";
                loadProfile(profileForApi);
                int value = GetIntProperty("FramerateLimit");
                return Result.Success("OK|profile=" + Sanitize(profileForApi) + "|fps=" + value + "|rtss=" + Sanitize(rtssPath));
            }
        }

        private sealed class Result
        {
            public bool Ok { get; private set; }
            public string Text { get; private set; }
            private Result(bool ok, string text) { Ok = ok; Text = text; }
            public static Result Success(string text) => new Result(true, text);
            public static Result Fail(string text) => new Result(false, text);
        }

        private static void LoadApi(string dllPath)
        {
            module = LoadLibrary(dllPath);
            if (module == IntPtr.Zero)
                throw new InvalidOperationException("LoadLibrary nie załadował RTSSHooks64.dll. Win32=" + Marshal.GetLastWin32Error());

            loadProfile = GetDelegate<LoadProfileDelegate>("LoadProfile");
            saveProfile = GetDelegate<SaveProfileDelegate>("SaveProfile");
            getProfileProperty = GetDelegate<GetProfilePropertyDelegate>("GetProfileProperty");
            setProfileProperty = GetDelegate<SetProfilePropertyDelegate>("SetProfileProperty");
            updateProfiles = GetDelegate<UpdateProfilesDelegate>("UpdateProfiles");
            setFlags = GetDelegate<SetFlagsDelegate>("SetFlags");
            getFlags = GetDelegate<GetFlagsDelegate>();
        }

        private static T GetDelegate<T>(string name) where T : class
        {
            IntPtr ptr = GetProcAddress(module, name);
            if (ptr == IntPtr.Zero)
                throw new MissingMethodException("RTSSHooks64.dll nie udostępnia funkcji " + name + ".");
            return Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
        }

        private static int GetIntProperty(string name)
        {
            IntPtr ptr = Marshal.AllocHGlobal(4);
            try
            {
                Marshal.WriteInt32(ptr, 0);
                if (!getProfileProperty(name, ptr, 4))
                    throw new InvalidOperationException("GetProfileProperty nie odczytał " + name + ".");
                return Marshal.ReadInt32(ptr);
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        private static void SetIntProperty(string name, int value)
        {
            IntPtr ptr = Marshal.AllocHGlobal(4);
            try
            {
                Marshal.WriteInt32(ptr, value);
                if (!setProfileProperty(name, ptr, 4))
                    throw new InvalidOperationException("SetProfileProperty odrzucił " + name + "=" + value + ".");
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        private static void PostRtssUpdateMessage()
        {
            IntPtr hwnd = FindWindow(null, "RTSS");
            if (hwnd == IntPtr.Zero) hwnd = FindWindow(null, "RivaTuner Statistics Server");
            if (hwnd != IntPtr.Zero) PostMessage(hwnd, WM_RTSS_UPDATESETTINGS, IntPtr.Zero, IntPtr.Zero);
        }

        private static void EnsureRtssRunning(string rtssPath)
        {
            if (Process.GetProcessesByName("RTSS").Length > 0) return;
            string exe = Path.Combine(rtssPath, "RTSS.exe");
            if (!File.Exists(exe)) return;
            Process.Start(exe);
            for (int i = 0; i < 15; i++)
            {
                Thread.Sleep(200);
                if (Process.GetProcessesByName("RTSS").Length > 0) return;
            }
        }

        private static string FindRtssPath()
        {
            string path = ReadInstallPath(RegistryView.Registry32);
            if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path)) return path;
            path = ReadInstallPath(RegistryView.Registry64);
            if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path)) return path;

            string[] candidates = {
                @"C:\Program Files (x86)\RivaTuner Statistics Server",
                @"C:\Program Files\RivaTuner Statistics Server"
            };
            foreach (string candidate in candidates)
                if (File.Exists(Path.Combine(candidate, "RTSSHooks64.dll"))) return candidate;
            return null;
        }

        private static string ReadInstallPath(RegistryView view)
        {
            try
            {
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                using (RegistryKey key = baseKey.OpenSubKey(@"Software\Unwinder\RTSS"))
                {
                    if (key == null) return null;
                    return key.GetValue("InstallPath") as string ?? key.GetValue("InstallDir") as string;
                }
            }
            catch { return null; }
        }

        private static string Sanitize(string value)
        {
            if (value == null) return "global";
            return value.Replace("", " ").Replace("
", " ").Replace("|", "/");
        }
    }
}