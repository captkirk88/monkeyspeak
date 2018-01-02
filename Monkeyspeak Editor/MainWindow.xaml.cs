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

        public MainWindow()
        {
            InitializeComponent();
            //Logger.SuppressSpam = true;
            console = new ConsoleWindow();
            Logger.LogOutput = new MultiLogOutput(new ConsoleWindowLogOutput(console),
                new NotificationPanelLogOutput(Level.Error));

            NotificationManager.Added += notif =>
            {
                this.Dispatcher.Invoke(() => notif_badge.Badge = NotificationManager.Count);
                this.Dispatcher.Invoke(() =>
                {
                    notifs_list.Items.Add(new NotificationPanel(notif));
                    notifs_list.ScrollIntoView(notifs_list.Items[notifs_list.Items.Count - 1]);
                });
            };
            NotificationManager.Removed += notif =>
            {
                this.Dispatcher.Invoke(() => notif_badge.Badge = NotificationManager.Count);
                if (NotificationManager.Count == 0) notifs_flyout.IsOpen = false;
            };

            Editors.Instance.Added += editor => this.Dispatcher.Invoke(() => docs.Items.Add(editor));
            Editors.Instance.Removed += editor => this.Dispatcher.Invoke(() => docs.Items.Remove(editor));

            foreach (var col in Enum.GetNames(typeof(AppColor)))
            {
                style_chooser.Items.Add(col);
            }

            plugins = new DefaultPluginContainer();

            Editors.Instance.Add();

            Logger.Error("TEST");
            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            notifs_flyout.AutoCloseInterval = 3000;
            notifs_flyout.IsAutoCloseEnabled = false;

            plugins.Initialize();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            console.Close();
        }

        private void Console_Click(object sender, RoutedEventArgs e)
        {
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
            if (NotificationManager.Count > 0)
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
                    NotificationManager.Clear();
                });
            }
        }

        private void notifs_flyout_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void TriggerList_SelectionChanged(Tuple<string, string> kv)
        {
            if (Editors.Instance.Selected != null)
            {
                Editors.Instance.Selected.InsertLine(Editors.Instance.Selected.CaretLine, kv.Item1);
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
    }
}