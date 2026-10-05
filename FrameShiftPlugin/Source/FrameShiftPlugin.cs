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
            return new FpsSettings(this) { GameFps = Settings.GameFps, GameProfiles = Settings.GameProfiles };
        }

        private void NormalizeSettings()
        {
            if (Settings.GameFps == null) Settings.GameFps = new Dictionary<string, int>();
            if (Settings.GameProfiles == null) Settings.GameProfiles = new Dictionary<string, string>();
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            return args.Name == ElementName ? new FrameShiftControl(this) : null;
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (args.Games == null || args.Games.Count != 1) yield break;
            var game = args.Games[0];
            yield return new GameMenuItem { Description = "FrameShift", MenuSection = "", Action = _ => ShowFpsMenu(game) };
        }

        private void ShowFpsMenu(Game game)
        {
            var menu = new Window
            {
                Title = "FrameShift — " + game.Name,
                Width = 420, Height = 340,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White, Foreground = Brushes.Black
            };
            var current = GetGameFps(game);
            var panel = new StackPanel { Margin = new Thickness(22) };
            panel.Children.Add(new TextBlock { Text = "FrameShift", FontSize = 22, FontWeight = FontWeights.Bold, Foreground = Brushes.Black, Margin = new Thickness(0,0,0,4) });
            panel.Children.Add(new TextBlock
            {
                Text = current > 0 ? "Current limit: " + current + " FPS" : "Current limit: OFF",
                FontSize = 16, FontWeight = FontWeights.SemiBold, Foreground = Brushes.DimGray, Margin = new Thickness(0,0,0,18)
            });

            foreach (var fps in new[] { 30, 40, 60, 120 })
            {
                var selected = fps == current;
                var b = new Button
                {
                    Content = selected ? "✓  " + fps + " FPS  —  ACTIVE" : fps + " FPS",
                    FontSize = 16, FontWeight = selected ? FontWeights.Bold : FontWeights.Normal,
                    Height = 42, Margin = new Thickness(0,3,0,3),
                    Background = selected ? Brushes.LightGreen : Brushes.WhiteSmoke,
                    Foreground = Brushes.Black, BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1)
                };
                var selectedFps = fps;
                b.Click += (s,e) => { if (SetRtssProfileForGame(game, selectedFps, true)) { SetGameFps(game, selectedFps); menu.Close(); } };
                panel.Children.Add(b);
            }

            var off = new Button
            {
                Content = current == 0 ? "✓  OFF  —  ACTIVE" : "OFF  —  disable limit",
                FontSize = 14, FontWeight = current == 0 ? FontWeights.Bold : FontWeights.Normal,
                Height = 38, Margin = new Thickness(0,12,0,0),
                Background = current == 0 ? Brushes.LightGreen : Brushes.WhiteSmoke,
                Foreground = Brushes.Black, BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1)
            };
            off.Click += (s,e) => { if (SetRtssProfileForGame(game, 0, true)) { SetGameFps(game, 0); menu.Close(); } };
            panel.Children.Add(off);
            menu.Content = panel;
            menu.ShowDialog();
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
            if (Settings.GameProfiles.TryGetValue(key, out saved) && IsValidProfileName(saved)) return saved;

            string resolved = TryResolveFromPlayAction(game);
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
            else if (full.Contains("\\binaries\\win64\\") || full.Contains("\\binaries\\win32\\")) score += 28;
            else if (full.Contains("\\bin\\")) score += 18;
            foreach (var token in tokens) if (token.Length >= 3 && normalized.Contains(token)) score += 18;
            string compactGame = NormalizeTokenString(gameName).Replace(" ", "");
            if (!string.IsNullOrWhiteSpace(compactGame) && normalized.Contains(compactGame)) score += 40;
            if (lower.EndsWith("-shipping")) score += 8;
            if (lower.Contains("game")) score += 5;
            if (lower.Contains("win64")) score += 4;
            return score;
        }

        private bool IsLikelyHelper(string name)
        {
            string n = (name ?? "").ToLowerInvariant();
            string[] bad = { "unins","uninstall","setup","installer","crashreport","crashpad","unitycrashhandler","ueprereq","dxsetup","vcredist","vc_redist","easyanticheat","eac","battleye","cefsharp","updater","update","repair","redist","prereq","register","benchmark","server","dedicated" };
            return bad.Any(x => n.Contains(x));
        }

        private bool IsKnownLauncher(string profile)
        {
            string n = (profile ?? "").ToLowerInvariant();
            string[] names = { "steam.exe","steamwebhelper.exe","epicgameslauncher.exe","ealauncher.exe","eadesktop.exe","eaapp.exe","ubisoftconnect.exe","upc.exe","galaxyclient.exe","goggalaxy.exe","rockstarclientservice.exe","rockstarlauncher.exe","bethesdanetlauncher.exe","battlenet.exe","battle.net.exe","riotclientservices.exe","riotclient.exe","playnite.desktopapp.exe","playnite.fullscreenapp.exe" };
            return names.Contains(n);
        }

        private bool IsIgnoredDirectory(string name)
        {
            string n = (name ?? "").ToLowerInvariant();
            string[] dirs = { "redist","_commonredist","support","directx","dotnet","vcredist","easyanticheat","battleye","crashreportclient","__installer","installerdata","tools" };
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
            string profile = GetGameRtssProfile(args.Game);
            logger.Info(string.IsNullOrWhiteSpace(profile)
                ? "FrameShift: no EXE profile known yet for " + args.Game.Name
                : "FrameShift: using saved RTSS profile " + profile + " = " + fps + " FPS for " + args.Game.Name);
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
                    string profile = null;
                    try { profile = GetProfileFromPath(process.MainModule.FileName); } catch { }
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
            return game != null && Settings.GameProfiles != null && Settings.GameProfiles.TryGetValue(game.Id.ToString(), out profile) && IsValidProfileName(profile) ? profile : null;
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
                MessageBox.Show("Unable to set the RTSS limit.\n\n" + ex.Message, "FrameShift", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public bool SetRtssProfileForGame(Game game, int fps, bool showError)
        {
            string profile = GetGameRtssProfile(game);
            if (string.IsNullOrWhiteSpace(profile))
            {
                if (showError) MessageBox.Show("FrameShift could not determine the game's EXE.\n\nCheck the game's Play action in Playnite.", "FrameShift", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                if (showError) MessageBox.Show("Unable to save the RTSS profile.\n\n" + ex.Message, "FrameShift", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        private bool IsValidFps(int fps) { return fps == 0 || fps == 30 || fps == 40 || fps == 60 || fps == 120; }

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
                Arguments = "/Run /TN \"FrameShift RTSS Bridge\"",
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