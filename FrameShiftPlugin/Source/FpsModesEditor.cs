using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Playnite.SDK;

namespace FrameShift
{
    /// <summary>
    /// Desktop-only dialog (Main menu → Extensions → FrameShift → Customize FPS modes…) for the four
    /// FPS modes shown in every FrameShift menu. Uses a Playnite window so it follows the Desktop theme.
    /// </summary>
    internal static class FpsModesEditor
    {
        private static readonly Brush ErrorBrush = new SolidColorBrush(Color.FromRgb(232, 86, 86));

        public static void Show(FrameShiftPlugin plugin)
        {
            var api = plugin.PlayniteApi;
            var window = api.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = false,
                ShowCloseButton = true
            });
            window.Title = "FrameShift — " + Loc.Get("ModesTitle");
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.ResizeMode = ResizeMode.NoResize;
            window.FlowDirection = Loc.FlowDirection;
            window.Owner = api.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // TextBlocks are not styled by the Playnite theme, so take its text colour explicitly.
            var text = window.TryFindResource("TextBrush") as SolidColorBrush ?? new SolidColorBrush(Colors.White);
            window.Content = BuildContent(plugin.GetFpsModes(), text.Color,
                values => { plugin.SaveFpsModes(values); window.Close(); },
                () => window.Close());
            window.ShowDialog();
        }

        /// <summary>Builds the editor content; separate from Show so it can be previewed without Playnite.</summary>
        internal static FrameworkElement BuildContent(IList<int> modes, Color textColor, Action<List<int>> save, Action cancel)
        {
            var text = new SolidColorBrush(textColor);
            var cardBackground = new SolidColorBrush(Color.FromArgb(18, textColor.R, textColor.G, textColor.B));
            var cardBorder = new SolidColorBrush(Color.FromArgb(40, textColor.R, textColor.G, textColor.B));

            var root = new StackPanel { Margin = new Thickness(24, 20, 24, 20), Width = 400 };
            root.Children.Add(new TextBlock
            {
                Text = Loc.Get("ModesTitle"),
                FontSize = 22,
                FontWeight = FontWeights.SemiBold,
                Foreground = text
            });
            root.Children.Add(new TextBlock
            {
                Text = Loc.Format("ModesHint", FpsModeRules.MinFps, FpsModeRules.MaxFps),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                Foreground = text,
                Opacity = 0.7,
                Margin = new Thickness(0, 6, 0, 16)
            });

            var boxes = new List<TextBox>();
            var cards = new List<Border>();
            var grid = new UniformGrid { Columns = 2, Margin = new Thickness(-6, 0, -6, 0) };
            for (int i = 0; i < FpsModeRules.ModeCount; i++)
            {
                var box = new TextBox
                {
                    Text = modes[i].ToString(),
                    MaxLength = 3,
                    FontSize = 22,
                    MinWidth = 90,
                    TextAlignment = TextAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                box.PreviewTextInput += (s, e) => e.Handled = !e.Text.All(char.IsDigit);
                DataObject.AddPastingHandler(box, (s, e) =>
                {
                    var pasted = e.DataObject.GetData(typeof(string)) as string;
                    if (pasted == null || !pasted.Trim().All(char.IsDigit)) e.CancelCommand();
                });
                box.GotKeyboardFocus += (s, e) => ((TextBox)s).SelectAll();

                var unit = new TextBlock { Text = "FPS", FontSize = 16, Foreground = text, Opacity = 0.7, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
                var row = new DockPanel();
                DockPanel.SetDock(unit, Dock.Right);
                row.Children.Add(unit);
                row.Children.Add(box);

                var content = new StackPanel();
                content.Children.Add(new TextBlock { Text = Loc.Format("ModeLabel", i + 1), FontSize = 12, Foreground = text, Opacity = 0.65, Margin = new Thickness(2, 0, 0, 6) });
                content.Children.Add(row);

                var card = new Border
                {
                    Background = cardBackground,
                    BorderBrush = cardBorder,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(14, 10, 14, 12),
                    Margin = new Thickness(6),
                    Child = content
                };
                grid.Children.Add(card);
                boxes.Add(box);
                cards.Add(card);
            }
            root.Children.Add(grid);

            var error = new TextBlock
            {
                Text = Loc.Format("InvalidModes", FpsModeRules.MinFps, FpsModeRules.MaxFps),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                Foreground = ErrorBrush,
                Margin = new Thickness(0, 10, 0, 0),
                Visibility = Visibility.Collapsed
            };
            root.Children.Add(error);

            var buttons = new DockPanel { LastChildFill = false, Margin = new Thickness(0, 18, 0, 0) };
            var reset = new Button { Content = Loc.Get("RestoreDefaults"), Padding = new Thickness(14, 6, 14, 6) };
            var saveButton = new Button { Content = Loc.Get("Save"), IsDefault = true, MinWidth = 96, Padding = new Thickness(16, 6, 16, 6), Margin = new Thickness(8, 0, 0, 0) };
            var cancelButton = new Button { Content = Loc.Get("Cancel"), IsCancel = true, MinWidth = 96, Padding = new Thickness(16, 6, 16, 6), Margin = new Thickness(8, 0, 0, 0) };
            DockPanel.SetDock(reset, Dock.Left);
            DockPanel.SetDock(cancelButton, Dock.Right);
            DockPanel.SetDock(saveButton, Dock.Right);
            buttons.Children.Add(reset);
            buttons.Children.Add(cancelButton);
            buttons.Children.Add(saveButton);
            root.Children.Add(buttons);

            // Live validation: invalid or duplicate values get a red card border and disable Save.
            Action validate = () =>
            {
                var values = boxes.Select(b => { int v; return int.TryParse((b.Text ?? "").Trim(), out v) ? v : -1; }).ToList();
                bool allValid = true;
                for (int i = 0; i < values.Count; i++)
                {
                    bool ok = values[i] > 0 && FpsModeRules.IsValidLimit(values[i]) && values.Count(v => v == values[i]) == 1;
                    cards[i].BorderBrush = ok ? cardBorder : ErrorBrush;
                    allValid &= ok;
                }
                error.Visibility = allValid ? Visibility.Collapsed : Visibility.Visible;
                saveButton.IsEnabled = allValid;
            };
            foreach (var box in boxes) box.TextChanged += (s, e) => validate();
            validate();

            reset.Click += (s, e) =>
            {
                for (int i = 0; i < boxes.Count; i++) boxes[i].Text = FpsModeRules.Defaults[i].ToString();
            };
            cancelButton.Click += (s, e) => cancel();
            saveButton.Click += (s, e) =>
            {
                var values = TryParse(boxes);
                if (values != null) save(values);
            };
            return root;
        }

        // Four distinct whole numbers in [MinFps, MaxFps], or null.
        private static List<int> TryParse(IEnumerable<TextBox> boxes)
        {
            var values = new List<int>();
            foreach (var box in boxes)
            {
                int fps;
                if (!int.TryParse((box.Text ?? "").Trim(), out fps)) return null;
                values.Add(fps);
            }
            return FpsModeRules.IsValidModeSet(values) ? values : null;
        }
    }
}
