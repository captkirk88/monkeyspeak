using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Monkeyspeak.Editor.Commands;
using Monkeyspeak.Editor.Controls;
using Monkeyspeak.Editor.Interfaces.Plugins;
using Monkeyspeak.Editor.Logging;
using Monkeyspeak.Editor.Notifications;
using Monkeyspeak.Editor.Notifications.Controls;
using Monkeyspeak.Editor.Plugins;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private IPluginContainer plugins;

        public MainWindow(params string[] files)
        {
            InitializeComponent();
            //Logger.SuppressSpam = true;
            console = new ConsoleWindow();
            ((MultiLogOutput)Logger.LogOutput).Add(new NotificationPanelLogOutput(Level.Error), new ConsoleWindowLogOutput(console));

            NotificationManager.Instance.Added += notif =>
            {
                this.Dispatcher.Invoke(() =>
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
            };
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

            plugins = new DefaultPluginContainer();

            Loaded += MainWindow_Loaded;

            if (files != null && files.Length > 0)
                foreach (var file in files)
                {
                    if (!string.IsNullOrEmpty(file))
                        new OpenFileCommand().Execute(file);
                }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            notifs_flyout.AutoCloseInterval = 3000;
            notifs_flyout.IsAutoCloseEnabled = false;

            if (Editors.Instance.IsEmpty)
                new NewEditorCommand().Execute(null);

            NotificationManager.Instance.AddNotification(new WelcomeNotification());
            plugins.Initialize();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            console.Close();
            new ExitCommand().Execute(null);
        }

        private void Console_Click(object sender, RoutedEventArgs e)
        {
            // TODO create console.Toggle() method
            if (console.Visibility != Visibility.Visible)
            {
                console.Show();
            }
            else
            {
                console.Hide();
            }
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

        private void TriggerList_SelectionChanged(string trigger, string lib)
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
            Dispatcher.InvokeAsync(() =>
            {
                var selectedEditor = ((EditorControl)((MetroAnimatedTabControl)sender).SelectedItem);
                Editors.Instance.Selected = selectedEditor;
            });
        }

        public void SetColor(AppColor color)
        {
            this.Dispatcher.Invoke(() =>
            {
                Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
                ThemeManager.ChangeAppStyle(Application.Current.Resources,
                                        ThemeManager.GetAccent(Enum.GetName(typeof(AppColor), color)),
                                        appStyle.Item1);
            });
        }

        public AppColor GetColor()
        {
            Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
            if (Enum.TryParse(appStyle.Item2.Name, out AppColor color))
                return color;
            else return AppColor.Brown;
        }

        public void SetTheme(AppTheme accent)
        {
            this.Dispatcher.Invoke(() =>
            {
                Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
                ThemeManager.ChangeAppStyle(Application.Current.Resources,
                                        appStyle.Item2,
                                        ThemeManager.GetAppTheme($"Base{Enum.GetName(typeof(AppTheme), accent)}"));
            });
        }

        public AppTheme GetTheme()
        {
            Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
            Enum.TryParse(appStyle.Item1.Name.Replace("Base", ""), out AppTheme theme);
            return theme;
        }

        private void RestorePlugins_Click(object sender, RoutedEventArgs e)
        {
            plugins.Unload();
            plugins = new DefaultPluginContainer();
            plugins.Initialize();
        }
    }
}