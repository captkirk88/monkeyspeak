using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Monkeyspeak.Editor.Commands;
using Monkeyspeak.Editor.Controls;
using Monkeyspeak.Editor.HelperClasses;
using Monkeyspeak.Editor.Interfaces.Plugins;
using Monkeyspeak.Editor.Logging;
using Monkeyspeak.Editor.Notifications;
using Monkeyspeak.Editor.Notifications.Controls;
using Monkeyspeak.Editor.Plugins;
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
using System.Windows.Threading;

namespace Monkeyspeak.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ConsoleWindow console;

        public MainWindow(params string[] files)
        {
            InitializeComponent();
            //Logger.SuppressSpam = true;
            console = new ConsoleWindow();
            ((MultiLogOutput)Logger.LogOutput).Add(new NotificationPanelLogOutput(Level.Error), new ConsoleWindowLogOutput(console));

            NotificationManager.Instance.Added += notif => this.Dispatcher.Invoke(() =>
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
            });
            Editors.Instance.Removed += editor => this.Dispatcher.Invoke(() => docs.Items.Remove(editor));

            foreach (var col in Enum.GetNames(typeof(AppColor)))
            {
                style_chooser.Items.Add(col);
            }

            foreach (var theme in Enum.GetNames(typeof(AppTheme)))
            {
                theme_chooser.Items.Add(theme);
            }

            Loaded += MainWindow_Loaded;

            // Settings
            var settings = Properties.Settings.Default;
            Left = settings.WindowPosition.X;
            Top = settings.WindowPosition.Y;
            Width = settings.WindowSizeWidth;
            Height = settings.WindowSizeHeight;
            WindowState = settings.WindowState;
            SetColor(settings.Color);
            SetTheme(settings.Theme);

            // Load files passed into the program and from last session
            if (files != null && files.Length > 0)
                foreach (var file in files)
                {
                    if (!string.IsNullOrEmpty(file) && System.IO.File.Exists(file))
                        new OpenFileCommand().Execute(file);
                }

            if (!string.IsNullOrWhiteSpace(settings.LastSession))
                foreach (var file in settings.LastSession.Split(','))
                {
                    if (!string.IsNullOrEmpty(file) && System.IO.File.Exists(file))
                        new OpenFileCommand().Execute(file);
                }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                e.Handled = true;
                notifs_flyout.AutoCloseInterval = 3000;
                notifs_flyout.IsAutoCloseEnabled = false;

                if (Editors.Instance.IsEmpty)
                    new NewEditorCommand().Execute(null);

                NotificationManager.Instance.AddNotification(new WelcomeNotification());
                Plugins.Plugins.Initialize();
            });
            Dispatcher.Invoke(async () => await Check());
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            console.Close();
            if (sender is MainWindow)
            {
                new ExitCommand().Execute(null);
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

        private void TriggerList_SelectionChanged(string trigger, string description, string lib)
        {
            if (Editors.Instance.Selected != null)
            {
                if (Editors.Instance.Selected.CaretLine > 0)
                    Editors.Instance.Selected.InsertAtCaretLine(trigger);
                else Editors.Instance.Selected.AddLine(trigger);
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
                SetColor(col);
            }
        }

        private void theme_chooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Enum.TryParse(theme_chooser.SelectedItem.ToString(), out AppTheme theme))
            {
                SetTheme(theme);
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
            Plugins.Plugins.Unload();
            Plugins.Plugins.Initialize();
        }

        private void settingsDialog_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog dialog = new SettingsDialog();
            var settings = dialog.settingsProps.SelectedObject as Properties.Settings;
            settings.SettingsSaving += Settings_Saving;
            if (dialog.ShowDialog() ?? true)
            {
                settings.SettingsSaving -= Settings_Saving;
            }
        }

        private void Settings_Saving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var settings = sender as Properties.Settings;
            SetColor(settings.Color);
            SetTheme(settings.Theme);
            WindowState = settings.WindowState;
            Width = settings.WindowSizeWidth;
            Height = settings.WindowSizeHeight;
            WindowState = settings.WindowState;
        }

        private void mainButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).ContextMenu.IsOpen = true;
        }
    }
}