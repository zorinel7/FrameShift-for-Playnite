using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Playnite.SDK.Controls;
using Playnite.SDK.Models;

namespace FrameShift
{
    public partial class FrameShiftControl : PluginUserControl
    {
        private readonly FrameShiftPlugin plugin;
        private FpsMenuWindow menuWindow;

        public FrameShiftControl(FrameShiftPlugin plugin)
        {
            this.plugin = plugin;
            InitializeComponent();
            PreviewKeyDown += FrameShiftControl_PreviewKeyDown;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            base.GameContextChanged(oldContext, newContext);
            if (menuWindow != null)
            {
                try { menuWindow.Close(); } catch { }
                menuWindow = null;
            }
        }

        private void FpsButton_Click(object sender, RoutedEventArgs e)
        {
            if (menuWindow != null) return;
            var game = plugin.GetCurrentGame(GameContext);
            if (game == null)
            {
                plugin.LogInfo("FrameShift: no current game found for the control.");
                return;
            }
            OpenMenu(game);
        }

        private void OpenMenu(Game game)
        {
            var owner = Window.GetWindow(this);
            var current = plugin.GetGameFps(game);
            menuWindow = new FpsMenuWindow(this, plugin, game, current, owner);
            menuWindow.Closed += delegate { menuWindow = null; };
            if (owner != null) menuWindow.Owner = owner;
            menuWindow.ShowDialog();
        }

        private void FrameShiftControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Return)
            {
                e.Handled = true;
                FpsButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else if (e.Key == Key.Escape || e.Key == Key.Back)
            {
                if (menuWindow != null)
                {
                    e.Handled = true;
                    menuWindow.Close();
                }
            }
        }

        internal static Brush GetBrush(FrameworkElement source, string resourceKey, Brush fallback)
        {
            try
            {
                var resource = source.TryFindResource(resourceKey);
                var brush = resource as Brush;
                if (brush != null) return brush;
            }
            catch { }
            return fallback;
        }

        internal static FontFamily GetFont(FrameworkElement source, string resourceKey, FontFamily fallback)
        {
            try
            {
                var resource = source.TryFindResource(resourceKey);
                var font = resource as FontFamily;
                if (font != null) return font;
            }
            catch { }
            return fallback;
        }
    }

    internal sealed class FpsMenuWindow : Window
    {
        private readonly FrameShiftControl ownerControl;
        private readonly FrameShiftPlugin plugin;
        private readonly Game game;
        private readonly int originalFps;
        private readonly FrameGamepadButton[] buttons;
        private bool confirmed;

        public FpsMenuWindow(FrameShiftControl ownerControl, FrameShiftPlugin plugin, Game game, int current, Window owner)
        {
            this.ownerControl = ownerControl;
            this.plugin = plugin;
            this.game = game;
            originalFps = current;

            Width = 500;
            Height = 520;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            WindowStartupLocation = owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;
            Focusable = true;

            CopyThemeResources();

            var root = new Border
            {
                Background = FindResource("ControlBackgroundBrush") as Brush ?? new SolidColorBrush(Color.FromRgb(58, 58, 58)),
                BorderBrush = FindResource("DefaultBorderBrush") as Brush ?? new SolidColorBrush(Color.FromRgb(68, 68, 68)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(28),
                Padding = new Thickness(28),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 18, Direction = 315, Opacity = 0.6, ShadowDepth = 7 }
            };

            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = "FrameShift",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontFamily = FindResource("FontDefaultLight") as FontFamily ?? new FontFamily("Segoe UI"),
                FontSize = 38,
                Foreground = FindResource("TextBrush") as Brush ?? Brushes.White,
                Margin = new Thickness(0, 0, 0, 2)
            });
            panel.Children.Add(new TextBlock
            {
                Text = game.Name,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 420,
                FontFamily = FindResource("FontDefaultLight") as FontFamily ?? new FontFamily("Segoe UI"),
                FontSize = 21,
                Foreground = FindResource("TextBrush") as Brush ?? Brushes.White,
                Opacity = 0.72,
                Margin = new Thickness(0, 0, 0, 18)
            });

            var options = new[] { 30, 40, 60, 120, 0 };
            buttons = new FrameGamepadButton[options.Length];
            for (var i = 0; i < options.Length; i++)
            {
                var fps = options[i];
                var button = new FrameGamepadButton
                {
                    Content = fps > 0 ? fps + " FPS" : "OFF",
                    Style = ownerControl.FindResource("FpsChoiceStyle") as Style,
                    Tag = fps
                };
                button.Click += MenuButton_Click;
                buttons[i] = button;
                panel.Children.Add(button);
            }

            root.Child = panel;
            Content = root;

            Loaded += delegate { SelectCurrent(current); Activate(); };
            PreviewKeyDown += FpsMenuWindow_PreviewKeyDown;
        }

        private void CopyThemeResources()
        {
            Resources["ControlBackgroundBrush"] = FrameShiftControl.GetBrush(ownerControl, "ControlBackgroundBrush", new SolidColorBrush(Color.FromRgb(58, 58, 58)));
            Resources["DefaultBorderBrush"] = FrameShiftControl.GetBrush(ownerControl, "DefaultBorderBrush", new SolidColorBrush(Color.FromRgb(68, 68, 68)));
            Resources["TextBrush"] = FrameShiftControl.GetBrush(ownerControl, "TextBrush", Brushes.White);
            Resources["TextBrushDark"] = FrameShiftControl.GetBrush(ownerControl, "TextBrushDark", Brushes.Gray);
            Resources["PrimaryBrush"] = FrameShiftControl.GetBrush(ownerControl, "PrimaryBrush", new SolidColorBrush(Color.FromRgb(56, 189, 248)));
            Resources["MainBackgourndBrush"] = FrameShiftControl.GetBrush(ownerControl, "MainBackgourndBrush", Brushes.Black);
            Resources["FontDefaultLight"] = FrameShiftControl.GetFont(ownerControl, "FontDefaultLight", new FontFamily("Segoe UI"));
        }

        private void SelectCurrent(int current)
        {
            for (var i = 0; i < buttons.Length; i++)
            {
                if (Convert.ToInt32(buttons[i].Tag) == current)
                {
                    var index = i;
                    Dispatcher.BeginInvoke(new Action(() => buttons[index].Focus()), DispatcherPriority.Input);
                    return;
                }
            }
            Dispatcher.BeginInvoke(new Action(() => buttons[0].Focus()), DispatcherPriority.Input);
        }

        private void FpsMenuWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Back)
            {
                e.Handled = true;
                Close();
                return;
            }
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                var current = Keyboard.FocusedElement as FrameGamepadButton;
                if (current == null) return;
                var index = Array.IndexOf(buttons, current);
                if (index < 0) index = 0;
                index += e.Key == Key.Down ? 1 : -1;
                if (index < 0) index = buttons.Length - 1;
                if (index >= buttons.Length) index = 0;
                buttons[index].Focus();
                e.Handled = true;
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameGamepadButton;
            if (button == null) return;
            var fps = Convert.ToInt32(button.Tag);
            if (!plugin.SetRtssProfileForGame(game, fps, true)) return;
            confirmed = true;
            plugin.SetGameFps(game, fps);
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (!confirmed && originalFps == plugin.GetGameFps(game)) return;
        }
    }
}