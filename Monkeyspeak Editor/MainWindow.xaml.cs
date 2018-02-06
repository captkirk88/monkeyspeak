using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Monkeyspeak.Editor.Collaborate;
using Monkeyspeak.Editor.Commands;
using Monkeyspeak.Editor.Controls;
using Monkeyspeak.Editor.Extensions;
using Monkeyspeak.Editor.HelperClasses;
using Monkeyspeak.Editor.Interfaces.Plugins;
using Monkeyspeak.Editor.Keybindings;
using Monkeyspeak.Editor.Logging;
using Monkeyspeak.Editor.Notifications;
using Monkeyspeak.Editor.Notifications.Controls;
using Monkeyspeak.Editor.Plugins;
using Monkeyspeak.Editor.Syntax;
using Monkeyspeak.Editor.Utils;
using Monkeyspeak.Extensions;
using Monkeyspeak.Lexical;
using Monkeyspeak.Logging;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Monkeyspeak.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ConsoleWindow console;

        public MainWindow(params string[] args)
        {
            InitializeComponent();
            //Logger.SuppressSpam = true;
            console = new ConsoleWindow();
            ((MultiLogOutput)Logger.LogOutput).Add(new NotificationPanelLogOutput(Level.Error), new ConsoleWindowLogOutput(console));

            Github.Initialize("captkirk88", "monkeyspeak");

            AllowDrop = true;
            PreviewDrop += (sender, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (var file in files)
                    {
                        MonkeyspeakCommands.Open.Execute(file);
                    }
                    e.Handled = true;
                }
            };

            NotificationManager.Instance.Added += notif => Dispatcher.Invoke(() =>
            {
                var count = NotificationManager.Instance.Count;
                if (count == 0)
                {
                    notifs_flyout.IsOpen = false;
                    notif_badge.Badge = "";
                }
                else notif_badge.Badge = count;

                notifs_list.Items.Add(new NotificationPanel(notif));
                notifs_flyout_scroll.ScrollToBottom();
            });
            NotificationManager.Instance.Removed += notif => Dispatcher.Invoke(() =>
            {
                var count = NotificationManager.Instance.Count;
                if (count == 0)
                {
                    notifs_flyout.IsOpen = false;
                    notif_badge.Badge = "";
                }
                else notif_badge.Badge = count;
            });

            Editors.Instance.Added += editor => this.Dispatcher.Invoke(() =>
            {
                if (!docs.Items.Contains(editor)) docs.Items.Add(editor);
                ((MetroAnimatedSingleRowTabControl)editor.Parent).SelectedItem = editor;
                editor.LineAdded += delegate { line_count.Text = editor.TriggerCount.ToString(); };
            });
            Editors.Instance.Removed += editor => this.Dispatcher.Invoke(() => docs.Items.Remove(editor));
            Editors.Instance.SelectionChanged += editor => this.Dispatcher.Invoke(() => line_count.Text = editor.TriggerCount.ToString());

            SyntaxChecker.Info += SyntaxChecker_Event;
            SyntaxChecker.Warning += SyntaxChecker_Event;
            SyntaxChecker.Error += SyntaxChecker_Event;
            SyntaxChecker.Cleared += SyntaxChecker_Cleared;
            SyntaxChecker.ClearedLine += SyntaxChecker_ClearedLine;

            errors_list.SelectionMode = SelectionMode.Extended;
            errors_list.PreviewKeyDown += (sender, e) =>
            {
                if (e.Key == Key.Delete)
                {
                    var items = errors_list.SelectedItems.Cast<object>().ToArray();
                    if (items.Length > 0)
                    {
                        foreach (var item in items)
                        {
                            errors_list.Items.Remove(item);
                        }
                        if (errors_list.Items.Count == 0)
                            errors_flyout.IsOpen = false;
                        e.Handled = true;
                    }
                }
            };
            errors_flyout.IsOpenChanged += (sender, e) =>
            {
                if (errors_flyout.IsOpen)
                    Editors.Instance.Selected?.textEditor?.Focus();
                else if (Intellisense.IsOpen)
                    Intellisense.GenerateTriggerListCompletion(Editors.Instance.Selected);
            };

            foreach (var col in Enum.GetNames(typeof(AppColor)))
            {
                style_chooser.Items.Add(col);
            }

            foreach (var theme in Enum.GetNames(typeof(AppTheme)))
            {
                theme_chooser.Items.Add(theme);
            }

            Loaded += MainWindow_Loaded;

            // Load files passed into the program and from last session
            ProcessArguments(args);
        }

        public void ProcessArguments(string[] args)
        {
            // Load files passed into the program and from last session
            if (args != null && args.Length > 0)
            {
                foreach (var arg in args)
                {
                    bool hasLineNumber = arg.ToLowerInvariant().IndexOf(":L") != -1;
                    string filePath = null;
                    if (hasLineNumber) filePath = arg.LeftOf(":L");
                    else filePath = arg;
                    if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath)) // is a file
                    {
                        if (!Editors.Instance.All.Any(e => e.CurrentFilePath == filePath))
                            MonkeyspeakCommands.Open.Execute(arg);
                    }
                    if (hasLineNumber)
                    {
                        var lineStr = arg.RightOf(":L");
                        if (int.TryParse(lineStr, out var line))
                        {
                            // find the editor with the file name
                            var editor = Editors.Instance.All.FirstOrDefault(e => e.CurrentFilePath == filePath);
                            if (editor != null)
                            {
                                var docLine = editor.textEditor.Document.GetLineByNumber(line);
                                if (docLine != null)
                                {
                                    editor.textEditor.TextArea.Caret.Line = line;
                                    editor.textEditor.ScrollToLine(line);
                                    editor.textEditor.Select(docLine.Offset, docLine.Length);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SyntaxChecker_Cleared(EditorControl editor)
        {
            errors_list.Items.Clear();
            errors_flyout.IsOpen = false;
        }

        private void SyntaxChecker_ClearedLine(EditorControl editor, int line)
        {
            var toClear = errors_list.Items.Cast<ListViewItem>().Where(item =>
            {
                if (item.Tag is SyntaxError error)
                {
                    if (error.Editor == editor && error.SourcePosition.Line == line)
                        return true;
                }
                return false;
            }).ToArray();
            foreach (var item in toClear) errors_list.Items.Remove(item);
            if (errors_list.Items.Count == 0) errors_flyout.IsOpen = false;
        }

        private void SyntaxChecker_Event(EditorControl editor, SyntaxError error)
        {
            if (error.Severity == SyntaxChecker.Severity.Warning && !Properties.Settings.Default.ShowWarnings) return;
            Brush brush = Brushes.White;
            switch (error.Severity)
            {
                case SyntaxChecker.Severity.Error:
                    brush = Brushes.Red;
                    break;

                case SyntaxChecker.Severity.Warning:
                    brush = Brushes.Yellow;
                    break;

                case SyntaxChecker.Severity.Info:
                    brush = Brushes.LightBlue;
                    break;
            }
            ListViewItem item = new ListViewItem
            {
                BorderBrush = brush,
                BorderThickness = new Thickness(2)
            };
            item.MouseDoubleClick += (sender, e) =>
            {
                editor.Focus();
                editor.textEditor.TextArea.Caret.Line = error.SourcePosition.Line;
                var line = editor.textEditor.Document.GetLineByOffset(editor.textEditor.CaretOffset);
                editor.textEditor.ScrollToLine(error.SourcePosition.Line);
                editor.textEditor.Select(line.Offset, line.Length);
                e.Handled = true;
            };
            item.PreviewKeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    editor.Focus();
                    editor.textEditor.TextArea.Caret.Line = error.SourcePosition.Line;
                    var line = editor.textEditor.Document.GetLineByOffset(editor.textEditor.CaretOffset);
                    editor.textEditor.ScrollToLine(error.SourcePosition.Line);
                    editor.textEditor.Select(line.Offset, line.Length);
                    e.Handled = true;
                }
            };
            item.ToolTip = "Double click to go to error.  Select item and press DELETE key to remove.";
            item.Tag = error;

            VirtualizingStackPanel content = new VirtualizingStackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            TextBlock lineInfo = new TextBlock { Text = $"Line {error.SourcePosition.Line}, Col {error.SourcePosition.Column}" };
            TextBlock source = new TextBlock { Text = System.IO.Path.GetFileName(editor.CurrentFilePath ?? editor.Title), Foreground = Brushes.DarkCyan };
            content.Children.Add(lineInfo);
            content.Children.Add(new Rectangle
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = 2,
                Margin = new Thickness(2),
                StrokeThickness = 4,
                Stroke = Brushes.Transparent,
                Fill = Brushes.White
            });
            content.Children.Add(source);
            content.Children.Add(new Rectangle
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = 2,
                Margin = new Thickness(2),
                StrokeThickness = 4,
                Stroke = Brushes.Transparent,
                Fill = Brushes.White
            });
            content.Children.Add(new TextBlock { IsHyphenationEnabled = true, Text = error.Exception.Message, FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic, Foreground = brush });
            item.Content = content;
            errors_list.Items.Add(item);
            if (errors_flyout.IsOpen == false && error.Severity == SyntaxChecker.Severity.Error ||
                (error.Severity == SyntaxChecker.Severity.Warning &&
                Properties.Settings.Default.AutoOpenOnWarning &&
                Properties.Settings.Default.ShowWarnings))
            {
                errors_flyout.IsOpen = true;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                e.Handled = true;
                // Settings
                var settings = Properties.Settings.Default;
                settings.Upgrade();
                Left = settings.WindowPosition.X;
                Top = settings.WindowPosition.Y;
                Width = settings.WindowSizeWidth;
                Height = settings.WindowSizeHeight;
                WindowState = settings.WindowState;
                SetColor(settings.Color);
                SetTheme(settings.Theme);
                theme_chooser.SelectedItem = settings.Theme;
                style_chooser.SelectedItem = settings.Color;

                if (settings.TriggerSplitterPosition > 0)
                    TopRow.Height = new GridLength(settings.TriggerSplitterPosition);
                else settings["TriggerSplitterPosition"] = TopRow.Height.Value;
                Intellisense.Enabled = settings.Intellisense;

                if (!string.IsNullOrWhiteSpace(settings.LastSession))
                    foreach (var file in settings.LastSession.Split(';'))
                    {
                        if (!string.IsNullOrEmpty(file) && System.IO.File.Exists(file))
                            MonkeyspeakCommands.Open.Execute(file);
                    }

                notifs_flyout.AutoCloseInterval = 3000;
                notifs_flyout.IsAutoCloseEnabled = false;

                if (Editors.Instance.IsEmpty)
                    new NewEditorCommand().Execute(null);

                HotkeyManager.ApplyChangesToInputBindings();

                NotificationManager.Instance.AddNotification(new WelcomeNotification(NotificationManager.Instance));
                Plugins.PluginsManager.Initialize();
            });
            Dispatcher.Invoke(async () => await Check());
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            if (console != null) console.Close();

            // save some application settings
            var settings = Properties.Settings.Default;
            settings.Upgrade();
            settings["TriggerSplitterPosition"] = TopRow.Height.Value;
            settings.Save();

            if (sender is MainWindow)
            {
                MonkeyspeakCommands.Exit.Execute(null);
            }
        }

        private void Console_Click(object sender, RoutedEventArgs e)
        {
            console.Toggle();
        }

        private void Notifications_Click(object sender, RoutedEventArgs e)
        {
            if (NotificationManager.Instance.Count > 0)
                notifs_flyout.IsOpen = true;
            else notifs_flyout.IsOpen = false;
        }

        private void Notifications_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                this.Dispatcher.Invoke(() =>
                {
                    notifs_flyout.IsOpen = false;
                    NotificationManager.Instance.Clear();
                });
            }
        }

        private void notifs_flyout_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void TriggerList_SelectionChanged(TriggerCompletionData data)
        {
            if (Editors.Instance.Selected != null)
            {
                if (Editors.Instance.Selected.CaretLine > 0)
                    Editors.Instance.Selected.InsertAtCaretLine(data.Prepare());
                else Editors.Instance.Selected.AddLine(data.Prepare());
            }
        }

        private void githubButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/captkirk88/monkeyspeak");
        }

        private void style_chooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Enum.TryParse(style_chooser.SelectedItem.ToString(), out AppColor col))
            {
                var settings = Properties.Settings.Default;
                settings.Upgrade();
                settings.Color = col;
                settings.SettingsSaving += Settings_Saving;
                settings.Save();
            }
        }

        private void theme_chooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Enum.TryParse(theme_chooser.SelectedItem.ToString(), out AppTheme theme))
            {
                var settings = Properties.Settings.Default;
                settings.Upgrade();
                settings.Theme = theme;
                settings.SettingsSaving += Settings_Saving;
                settings.Save();
            }
        }

        private void MetroAnimatedTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var selectedEditor = (((MetroAnimatedSingleRowTabControl)sender).SelectedItem as EditorControl);
                if (selectedEditor != null)
                    Editors.Instance.Selected = selectedEditor;
            });
        }

        public void SetColor(AppColor color)
        {
            Dispatcher.Invoke(() =>
            {
                Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(System.Windows.Application.Current);
                ThemeManager.ChangeAppStyle(System.Windows.Application.Current.Resources,
                                        ThemeManager.GetAccent(Enum.GetName(typeof(AppColor), color)),
                                        appStyle.Item1);
            });
            style_chooser.SelectedItem = color;
        }

        public AppColor GetColor()
        {
            Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(System.Windows.Application.Current);
            if (Enum.TryParse(appStyle.Item2.Name, out AppColor color))
                return color;
            else return AppColor.Brown;
        }

        public void SetTheme(AppTheme accent)
        {
            Dispatcher.Invoke(() =>
            {
                Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(System.Windows.Application.Current);
                ThemeManager.ChangeAppStyle(System.Windows.Application.Current.Resources,
                                        appStyle.Item2,
                                        ThemeManager.GetAppTheme($"Base{Enum.GetName(typeof(AppTheme), accent)}"));
            });
            style_chooser.SelectedItem = accent;
        }

        public AppTheme GetTheme()
        {
            Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(System.Windows.Application.Current);
            Enum.TryParse(appStyle.Item1.Name.Replace("Base", ""), out AppTheme theme);
            return theme;
        }

        public async Task Check()
        {
            var userVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var web = new WebClient();
            var release = await Github.GetLatestRelease();
            // in case internet is not connected or other issue return to prevent a nagging dialog
            if (release == null || release.Prerelease || release.Draft) return;
            var currentVersion = new Version(release.Body);
            if (currentVersion > userVersion)
            {
                foreach (var asset in release.Assets)
                {
                    if (asset.Name.Contains("Editor") && asset.Name.Contains("Binaries"))
                    {
                        var result = DialogManager.ShowModalMessageExternal(System.Windows.Application.Current.MainWindow as MetroWindow,
                                    "Update Found!", $"A update was found ({userVersion} -> {currentVersion}), would you like to download the latest version?", MessageDialogStyle.AffirmativeAndNegative,
                                    new MetroDialogSettings { DefaultButtonFocus = MessageDialogResult.Affirmative, AffirmativeButtonText = "Yes!", NegativeButtonText = "No" });

                        if (result == MessageDialogResult.Affirmative)
                        {
                            System.Diagnostics.Process.Start(asset.BrowserDownloadUrl);
                        }
                        break;
                    }
                }
            }
        }

        private void RestorePlugins_Click(object sender, RoutedEventArgs e)
        {
            Plugins.PluginsManager.Unload();
            Plugins.PluginsManager.Initialize();
        }

        private void settingsDialog_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog dialog = new SettingsDialog();
            var settings = dialog.settingsProps.SelectedObject as Properties.Settings;
            settings.SettingsSaving += Settings_Saving;
            dialog.ShowDialog();
        }

        private void Settings_Saving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            var settings = Properties.Settings.Default;
            settings.SettingsSaving -= Settings_Saving;
            SetColor(settings.Color);
            SetTheme(settings.Theme);
            WindowState = settings.WindowState;
            Width = settings.WindowSizeWidth;
            Height = settings.WindowSizeHeight;
            WindowState = settings.WindowState;
            Intellisense.Enabled = settings.Intellisense;

            HotkeyManager.ApplyChangesToInputBindings();
        }

        private void mainButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).ContextMenu.IsOpen = true;
        }

        private void errors_flyout_button_Click(object sender, RoutedEventArgs e)
        {
            errors_flyout.IsOpen = !errors_flyout.IsOpen;
        }
    }
}