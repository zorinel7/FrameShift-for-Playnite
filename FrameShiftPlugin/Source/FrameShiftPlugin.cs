using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.SDK.Events;

namespace FrameShift
{
    public class FpsSettings : ISettings
    {
        public Dictionary<string, int> GameFps { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, string> GameProfiles { get; set; } = new Dictionary<string, string>();
        // No initializer: the JSON deserializer would append saved values to a pre-filled list.
        // NormalizeSettings fills the defaults (30 / 40 / 60 / 120).
        public List<int> FpsModes { get; set; }
        [NonSerialized] private FrameShiftPlugin plugin;

        public FpsSettings() { }
        public FpsSettings(FrameShiftPlugin plugin) { this.plugin = plugin; }
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { if (plugin != null) plugin.SavePluginSettings(this); }
        public bool VerifySettings(out List<string> errors) { errors = new List<string>(); return true; }
    }

    public class FrameShiftPlugin : GenericPlugin
    {
        public override Guid Id { get; } = Guid.Parse("D8F0B2F6-2C8A-4E3E-9A7B-6F5F7E2A8B11");
        public const string SourceName = "FrameShift";
        public const string ElementName = "FrameShift";
        private readonly ILogger logger;
        public FpsSettings Settings { get; private set; }

        public FrameShiftPlugin(IPlayniteAPI api) : base(api)
        {
            logger = LogManager.GetLogger("FrameShift");
            Loc.Init(api.ApplicationSettings?.Language, ex => logger.Error(ex, "FrameShift: failed to load localization."));
            Settings = LoadPluginSettings<FpsSettings>() ?? new FpsSettings();
            NormalizeSettings();
            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { ElementName },
                SourceName = SourceName
            });
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            Settings = LoadPluginSettings<FpsSettings>() ?? new FpsSettings();
            NormalizeSettings();
            return new FpsSettings(this) { GameFps = Settings.GameFps, GameProfiles = Settings.GameProfiles, FpsModes = Settings.FpsModes };
        }

        private void NormalizeSettings()
        {
            if (Settings.GameFps == null) Settings.GameFps = new Dictionary<string, int>();
            if (Settings.GameProfiles == null) Settings.GameProfiles = new Dictionary<string, string>();
            Settings.FpsModes = FpsModeRules.Normalize(Settings.FpsModes);
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            return args.Name == ElementName ? new FrameShiftControl(this) : null;
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // First start after the .pext was installed or updated: install / refresh the bridge.
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    if (CheckBridge()) return;
                    PlayniteApi.MainView.UIDispatcher.Invoke(new Action(() => EnsureBridgeInstalled()));
                }
                catch (Exception ex) { logger.Error(ex, "FrameShift: bridge check failed."); }
            });
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            // FPS modes are edited in Desktop mode only; Fullscreen just uses them.
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                yield return new MainMenuItem { Description = Loc.Get("MenuCustomizeModes"), MenuSection = "@FrameShift", Action = _ => FpsModesEditor.Show(this) };
                yield return new MainMenuItem { Description = "-", MenuSection = "@FrameShift" };
            }
            yield return new MainMenuItem { Description = Loc.Get("MenuReinstallBridge"), MenuSection = "@FrameShift", Action = _ => ReinstallBridge() };
            yield return new MainMenuItem { Description = Loc.Get("MenuRemoveBridge"), MenuSection = "@FrameShift", Action = _ => RemoveBridge() };
        }

        private volatile bool bridgeReady;
        private bool bridgeDeclined;

        private bool CheckBridge()
        {
            if (!bridgeReady) bridgeReady = BridgeSetup.IsUpToDate();
            return bridgeReady;
        }

        /// <summary>Must run on the UI thread. Asks once per session, then shows the UAC prompt.</summary>
        private bool EnsureBridgeInstalled(bool userAction = false)
        {
            if (CheckBridge()) return true;
            if (bridgeDeclined && !userAction) return false;

            var answer = PlayniteApi.Dialogs.ShowMessage(
                Loc.Get("BridgeInstallPrompt") + "\n\n" + Loc.Get("BridgeUacNote"),
                "FrameShift", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (answer != MessageBoxResult.Yes) { bridgeDeclined = true; return false; }
            return InstallBridge();
        }

        private bool InstallBridge()
        {
            string error = BridgeSetup.RunElevated("install");
            bridgeReady = error == null && BridgeSetup.IsUpToDate();
            if (bridgeReady) { logger.Info("FrameShift: bridge installed to " + BridgeSetup.InstalledBridgePath); return true; }

            logger.Error("FrameShift: bridge install failed: " + (error ?? "installed bridge could not be verified."));
            PlayniteApi.Dialogs.ShowErrorMessage(Loc.Get("BridgeInstallFailed") + "\n\n" + (error ?? Loc.Get("BridgeNotVerified")), "FrameShift");
            return false;
        }

        private void ReinstallBridge()
        {
            bridgeReady = false;
            InstallBridge();
        }

        private void RemoveBridge()
        {
            string error = BridgeSetup.RunElevated("uninstall");
            bridgeReady = false;
            if (error == null) PlayniteApi.Dialogs.ShowMessage(Loc.Get("BridgeRemoved"), "FrameShift");
            else PlayniteApi.Dialogs.ShowErrorMessage(Loc.Get("BridgeRemoveFailed") + "\n\n" + error, "FrameShift");
        }

        // Game menu → FrameShift → the four FPS modes / OFF. Works in Desktop mode and in Fullscreen
        // with any theme (Fullscreen lists it under the game's Extensions menu), so no custom theme is needed.
        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (args.Games == null || args.Games.Count != 1) yield break;
            var game = args.Games[0];
            int current = GetGameFps(game);
            foreach (var fps in GetMenuOptions(current))
            {
                int selected = fps;
                string label = fps > 0 ? fps + " FPS" : (current == 0 ? Loc.Get("Off") : Loc.Get("OffDisable"));
                if (fps == current) label = "✓  " + label + "  —  " + Loc.Get("Active");
                yield return new GameMenuItem { Description = label, MenuSection = "FrameShift", Action = _ => ApplyFps(game, selected) };
            }
        }

        /// <summary>FPS modes (+ the current limit if it is no longer a mode) and OFF, for every FPS menu.</summary>
        public List<int> GetMenuOptions(int current)
        {
            return FpsModeRules.MenuOptions(Settings.FpsModes, current);
        }

        public List<int> GetFpsModes() { return FpsModeRules.Normalize(Settings.FpsModes); }

        public void SaveFpsModes(IList<int> modes)
        {
            Settings.FpsModes = FpsModeRules.Normalize(modes);
            SavePluginSettings(Settings);
            logger.Info("FrameShift: FPS modes = " + string.Join(", ", Settings.FpsModes));
        }

        private void ApplyFps(Game game, int fps)
        {
            if (SetRtssProfileForGame(game, fps, true)) SetGameFps(game, fps);
        }

        public Game GetCurrentGame(Game contextGame)
        {
            if (contextGame != null) return contextGame;
            try { return PlayniteApi.MainView.SelectedGames?.FirstOrDefault(); }
            catch (Exception ex) { logger.Error(ex, "FrameShift: failed to get selected game."); return null; }
        }

        public void LogInfo(string message) { logger.Info(message); }

        public int GetGameFps(Game game)
        {
            if (game == null) return 0;
            int value;
            return Settings.GameFps.TryGetValue(game.Id.ToString(), out value) ? value : 0;
        }

        public void SetGameFps(Game game, int fps)
        {
            if (game == null) return;
            string key = game.Id.ToString();
            if (fps <= 0) Settings.GameFps.Remove(key); else Settings.GameFps[key] = fps;
            SavePluginSettings(Settings);
        }

        public string GetGameRtssProfile(Game game)
        {
            if (game == null) return null;
            NormalizeSettings();
            string key = game.Id.ToString(), saved;
            // A saved launcher (e.g. gamelaunchhelper.exe from older versions) is ignored and re-resolved.
            if (Settings.GameProfiles.TryGetValue(key, out saved) && IsValidProfileName(saved) && !IsKnownLauncher(saved)) return saved;

            string resolved = TryResolveFromGameConfig(game);
            if (IsValidProfileName(resolved)) return RememberProfile(game, resolved);
            resolved = TryResolveFromPlayAction(game);
            if (IsValidProfileName(resolved)) return RememberProfile(game, resolved);
            resolved = TryResolveFromInstallDirectory(game);
            return IsValidProfileName(resolved) ? RememberProfile(game, resolved) : null;
        }

        private string RememberProfile(Game game, string profile)
        {
            if (game == null || !IsValidProfileName(profile)) return profile;
            Settings.GameProfiles[game.Id.ToString()] = profile;
            SavePluginSettings(Settings);
            logger.Info("FrameShift: RTSS profile for " + game.Name + " = " + profile);
            return profile;
        }

        // Xbox app / Microsoft Store (GDK) games start through gamelaunchhelper.exe, which is the same
        // file in every game. The real executable is listed in MicrosoftGame.config next to it.
        private string TryResolveFromGameConfig(Game game)
        {
            if (game == null || string.IsNullOrWhiteSpace(game.InstallDirectory)) return null;
            try
            {
                foreach (string dir in new[] { game.InstallDirectory, Path.Combine(game.InstallDirectory, "Content") })
                {
                    string config = Path.Combine(dir, "MicrosoftGame.config");
                    if (!File.Exists(config)) continue;
                    var doc = System.Xml.Linq.XDocument.Load(config);
                    foreach (var exe in doc.Descendants().Where(e => e.Name.LocalName == "Executable"))
                    {
                        string relative = (string)exe.Attribute("Name");
                        string profile = GetProfileFromPath(relative);
                        // Anti-cheat bootstrappers (start_protected_game.exe) are skipped; the directory
                        // scan then finds the game binary (e.g. Binaries\WinGDK\*.exe).
                        if (!IsValidProfileName(profile) || IsKnownLauncher(profile) || IsLikelyHelper(profile)) continue;
                        if (File.Exists(Path.Combine(dir, relative))) return profile;
                    }
                }
            }
            catch (Exception ex) { logger.Error(ex, "FrameShift: failed to read MicrosoftGame.config."); }
            return null;
        }

        private string TryResolveFromPlayAction(Game game)
        {
            try
            {
                if (game.GameActions == null) return null;
                var actions = game.GameActions.Where(a => a != null && a.Type == GameActionType.File)
                    .OrderByDescending(a => a.IsPlayAction).ToList();

                foreach (var action in actions)
                {
                    try
                    {
                        var expanded = PlayniteApi.ExpandGameVariables(game, action);
                        string profile = GetProfileFromPath(expanded != null ? expanded.Path : action.Path);
                        if (IsValidProfileName(profile) && !IsKnownLauncher(profile)) return profile;
                    }
                    catch { }
                }
            }
            catch (Exception ex) { logger.Error(ex, "FrameShift: failed to inspect game actions."); }
            return null;
        }

        private string GetProfileFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            try
            {
                string fileName = Path.GetFileName(path.Trim().Trim('"'));
                return !string.IsNullOrWhiteSpace(fileName) && fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? fileName : null;
            }
            catch { return null; }
        }

        private string TryResolveFromInstallDirectory(Game game)
        {
            if (game == null || string.IsNullOrWhiteSpace(game.InstallDirectory) || !Directory.Exists(game.InstallDirectory)) return null;
            try
            {
                string root = game.InstallDirectory;
                var tokens = Tokenize(game.Name);
                var candidates = new List<ExeCandidate>();
                foreach (var file in EnumerateExecutables(root, 4))
                {
                    string name = Path.GetFileName(file);
                    if (!IsValidProfileName(name) || IsKnownLauncher(name) || IsLikelyHelper(name)) continue;
                    int score = ScoreExecutable(tokens, game.Name, root, file);
                    if (score > 0) candidates.Add(new ExeCandidate { Name = name, Path = file, Score = score });
                }
                var best = candidates.OrderByDescending(x => x.Score).FirstOrDefault();
                return best != null && best.Score >= 35 ? best.Name : null;
            }
            catch (Exception ex) { logger.Error(ex, "FrameShift: failed to scan game directory."); return null; }
        }

        private IEnumerable<string> EnumerateExecutables(string root, int maxDepth)
        {
            var queue = new Queue<Tuple<string,int>>();
            queue.Enqueue(Tuple.Create(root, 0));
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                string[] files = null;
                try { files = Directory.GetFiles(item.Item1, "*.exe"); } catch { }
                if (files != null) foreach (var file in files) yield return file;
                if (item.Item2 >= maxDepth) continue;
                string[] dirs = null;
                try { dirs = Directory.GetDirectories(item.Item1); } catch { }
                if (dirs == null) continue;
                foreach (var dir in dirs)
                    if (!IsIgnoredDirectory(Path.GetFileName(dir))) queue.Enqueue(Tuple.Create(dir, item.Item2 + 1));
            }
        }

        private int ScoreExecutable(List<string> tokens, string gameName, string root, string path)
        {
            string file = Path.GetFileNameWithoutExtension(path) ?? "";
            string lower = file.ToLowerInvariant();
            string normalized = NormalizeTokenString(file);
            int score = 10;
            string full = path.ToLowerInvariant();
            if (string.Equals(Path.GetDirectoryName(path), root, StringComparison.OrdinalIgnoreCase)) score += 45;
            else if (full.Contains("\\binaries\\win64\\") || full.Contains("\\binaries\\win32\\") || full.Contains("\\binaries\\wingdk\\")) score += 28;
            else if (full.Contains("\\bin\\")) score += 18;
            foreach (var token in tokens) if (token.Length >= 3 && normalized.Contains(token)) score += 18;
            string compactGame = NormalizeTokenString(gameName).Replace(" ", "");
            if (!string.IsNullOrWhiteSpace(compactGame) && normalized.Contains(compactGame)) score += 40;
            if (lower.EndsWith("-shipping")) score += 8;
            if (lower.Contains("game")) score += 5;
            if (lower.Contains("win64")) score += 4;
            // "Skate.exe" vs "ReSkateLauncher.exe" in the same folder: prefer the game, not its launcher.
            if (lower.Contains("launcher")) score -= 40;
            return score;
        }

        private bool IsLikelyHelper(string name)
        {
            string n = (name ?? "").ToLowerInvariant();
            string[] bad = { "unins","uninstall","setup","installer","crashreport","crashpad","unitycrashhandler","ueprereq","dxsetup","vcredist","vc_redist","easyanticheat","eac","battleye","cefsharp","updater","update","repair","redist","prereq","register","benchmark","server","dedicated","shadercompiler","anticheat","cefsubprocess" };
            return bad.Any(x => n.Contains(x));
        }

        private bool IsKnownLauncher(string profile)
        {
            string n = (profile ?? "").ToLowerInvariant();
            string[] names = { "steam.exe","steamwebhelper.exe","epicgameslauncher.exe","ealauncher.exe","eadesktop.exe","eaapp.exe","ubisoftconnect.exe","upc.exe","galaxyclient.exe","goggalaxy.exe","rockstarclientservice.exe","rockstarlauncher.exe","bethesdanetlauncher.exe","battlenet.exe","battle.net.exe","riotclientservices.exe","riotclient.exe","playnite.desktopapp.exe","playnite.fullscreenapp.exe",
                // Game-specific launcher stubs and bootstrappers that never render the game.
                "gamelaunchhelper.exe","start_protected_game.exe","xboxpcapp.exe","gamingservices.exe","launcher.exe","mcclauncher.exe","playrdr2.exe" };
            return names.Contains(n);
        }

        private bool IsIgnoredDirectory(string name)
        {
            string n = (name ?? "").ToLowerInvariant();
            string[] dirs = { "redist","_commonredist","support","directx","dotnet","vcredist","easyanticheat","battleye","crashreportclient","__installer","installerdata","tools","embeddedxvd","custominstall" };
            return dirs.Contains(n) || n.StartsWith(".staging", StringComparison.OrdinalIgnoreCase);
        }

        private List<string> Tokenize(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? new List<string>() :
                NormalizeTokenString(text).Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Length >= 3).ToList();
        }

        private string NormalizeTokenString(string text)
        {
            if (text == null) return "";
            var result = new System.Text.StringBuilder();
            bool separator = false;
            foreach (char ch in text.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(ch)) { result.Append(ch); separator = false; }
                else if (!separator) { result.Append(' '); separator = true; }
            }
            return result.ToString().Trim();
        }

        private bool IsValidProfileName(string profile)
        {
            return !string.IsNullOrWhiteSpace(profile) &&
                   profile.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) &&
                   profile.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
                   profile.IndexOf('|') < 0 && profile.IndexOf('"') < 0;
        }

        private sealed class ExeCandidate { public string Name; public string Path; public int Score; }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            if (args?.Game == null) return;
            int fps = GetGameFps(args.Game);
            if (fps <= 0) return;
            string previous;
            NormalizeSettings();
            Settings.GameProfiles.TryGetValue(args.Game.Id.ToString(), out previous);
            string profile = GetGameRtssProfile(args.Game);
            if (string.IsNullOrWhiteSpace(profile))
            {
                logger.Info("FrameShift: no EXE profile known yet for " + args.Game.Name);
                return;
            }
            if (!string.Equals(previous, profile, StringComparison.OrdinalIgnoreCase))
            {
                // The EXE changed (e.g. a saved launcher was replaced): the limit lives in the RTSS
                // profile of the old EXE, so write it to the new one before the game starts.
                logger.Info("FrameShift: RTSS profile for " + args.Game.Name + " changed from " + (previous ?? "(none)") + " to " + profile + "; applying " + fps + " FPS.");
                SendRtssProfileLimit(profile, fps, false);
                return;
            }
            logger.Info("FrameShift: using saved RTSS profile " + profile + " = " + fps + " FPS for " + args.Game.Name);
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            if (args?.Game == null || args.StartedProcessId <= 0) return;
            try
            {
                int fps = GetGameFps(args.Game);
                if (fps <= 0 || GetSavedProfile(args.Game) != null) return;
                using (var process = System.Diagnostics.Process.GetProcessById(args.StartedProcessId))
                {
                    // Playnite is 32-bit: Process.MainModule throws for 64-bit games, so ask Windows
                    // for the image path instead.
                    string profile = GetProfileFromPath(GetProcessImagePath(process.Id));
                    if (!IsValidProfileName(profile) || IsKnownLauncher(profile)) return;
                    RememberProfile(args.Game, profile);
                    SendRtssProfileLimit(profile, fps, false);
                }
            }
            catch (Exception ex) { logger.Error(ex, "FrameShift: failed to learn EXE from started process."); }
        }

        private string GetSavedProfile(Game game)
        {
            string profile;
            return game != null && Settings.GameProfiles != null && Settings.GameProfiles.TryGetValue(game.Id.ToString(), out profile) && IsValidProfileName(profile) && !IsKnownLauncher(profile) ? profile : null;
        }

        private const uint ProcessQueryLimitedInformation = 0x1000;
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint access, bool inheritHandle, int processId);
        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        private static extern bool QueryFullProcessImageName(IntPtr process, int flags, System.Text.StringBuilder exeName, ref int size);
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        private static string GetProcessImagePath(int processId)
        {
            IntPtr handle = OpenProcess(ProcessQueryLimitedInformation, false, processId);
            if (handle == IntPtr.Zero) return null;
            try
            {
                var buffer = new System.Text.StringBuilder(1024);
                int size = buffer.Capacity;
                return QueryFullProcessImageName(handle, 0, buffer, ref size) ? buffer.ToString() : null;
            }
            finally { CloseHandle(handle); }
        }

        public void SendRtssHotkey(int fps) { SendRtssLimit(fps); }

        public void SendRtssLimit(int fps)
        {
            if (!IsValidFps(fps)) return;
            try
            {
                string response = SendToElevatedBridge("", fps, 5000);
                if (string.IsNullOrWhiteSpace(response) || !response.StartsWith("OK|", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(response ?? "No response from FrameShiftBridge.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "FrameShift: RTSS bridge error.");
                PlayniteApi.Dialogs.ShowErrorMessage(Loc.Get("ErrorGlobal") + "\n\n" + ex.Message, "FrameShift");
            }
        }

        public bool SetRtssProfileForGame(Game game, int fps, bool showError)
        {
            if (showError && !EnsureBridgeInstalled(userAction: true)) return false;
            string profile = GetGameRtssProfile(game);
            if (string.IsNullOrWhiteSpace(profile))
            {
                if (showError) PlayniteApi.Dialogs.ShowErrorMessage(Loc.Get("ErrorNoExe") + "\n\n" + Loc.Get("ErrorNoExeHint"), "FrameShift");
                return false;
            }
            return SendRtssProfileLimit(profile, fps, showError);
        }

        private bool SendRtssProfileLimit(string profile, int fps, bool showError)
        {
            if (!IsValidProfileName(profile) || !IsValidFps(fps)) return false;
            try
            {
                string response = SendToElevatedBridge(profile, fps, 5000);
                if (string.IsNullOrWhiteSpace(response) || !response.StartsWith("OK|", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(response ?? "No response from FrameShiftBridge.");
                logger.Info("FrameShift: saved RTSS profile " + profile + " = " + fps + " FPS");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "FrameShift: RTSS profile update failed.");
                if (showError) PlayniteApi.Dialogs.ShowErrorMessage(Loc.Get("ErrorRtssSave") + " " + profile + "\n\n" + ex.Message + "\n\n" + Loc.Format("ErrorReinstallHint", Loc.Get("MenuReinstallBridge")), "FrameShift");
                return false;
            }
        }

        private bool IsValidFps(int fps) { return FpsModeRules.IsValidLimit(fps); }

        private string SendToElevatedBridge(string profile, int fps, int timeoutMs)
        {
            lock (this)
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FrameShift");
                Directory.CreateDirectory(dir);
                string requestFile = Path.Combine(dir, "request.txt");
                string resultFile = Path.Combine(dir, "result.txt");
                string id = Guid.NewGuid().ToString("N");
                try { if (File.Exists(resultFile)) File.Delete(resultFile); } catch { }

                string safeProfile = profile ?? "";
                if (safeProfile.Contains("|")) throw new ArgumentException("Invalid RTSS profile name.");
                string tmp = requestFile + ".tmp." + System.Diagnostics.Process.GetCurrentProcess().Id;
                File.WriteAllText(tmp, id + "|" + safeProfile + "|" + fps);
                if (File.Exists(requestFile)) File.Delete(requestFile);
                File.Move(tmp, requestFile);
                StartBridgeTask();

                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    if (File.Exists(resultFile))
                    {
                        string result = null;
                        try { result = File.ReadAllText(resultFile).Trim(); } catch { }
                        string prefix = id + "|";
                        if (!string.IsNullOrWhiteSpace(result) && result.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            return result.Substring(prefix.Length);
                    }
                    Thread.Sleep(50);
                }
                return null;
            }
        }

        private void StartBridgeTask()
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = "/Run /TN \"" + BridgeSetup.TaskName + "\"",
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true
            };
            using (var process = System.Diagnostics.Process.Start(psi))
            {
                process.WaitForExit(3000);
                string err = process.StandardError.ReadToEnd();
                logger.Info("FrameShift: bridge task exit=" + process.ExitCode + (string.IsNullOrWhiteSpace(err) ? "" : " error=" + err.Trim()));
            }
        }
    }
}
