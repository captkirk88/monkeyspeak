using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Monkeyspeak.Editor.Logging;
using Monkeyspeak.Editor.Notifications;
using Monkeyspeak.Editor.Notifications.Controls;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Monkeyspeak.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ConsoleWindow console;

        public MainWindow()
        {
            InitializeComponent();
            console = new ConsoleWindow();
            Logger.LogOutput = new MultiLogOutput(new ConsoleWindowLogOutput(console), new NotificationPanelLogOutput());
            NotificationManager.Added += notif => notif_badge.Badge = NotificationManager.Count;
            NotificationManager.Removed += notif => notif_badge.Badge = NotificationManager.Count;
            NotificationManager.Added += notif =>
            {
                notifs_list.Items.Add(new NotificationPanel(notif));
                notifs_list.ScrollIntoView(notifs_list.Items[notifs_list.Items.Count - 1]);
            };

            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                await Task.Run(() =>
                {
                    for (int i = 0; i <= 1000; i++)
                    {
                        Logger.Info(i);
                    }
                });
            });
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
            notifs_container.IsOpen = !notifs_container.IsOpen;
        }

        private void Notifications_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.Dispatcher.Invoke(() =>
                {
                    notifs_container.IsOpen = false;
                    NotificationManager.Clear();
                });
            }
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void notifs_container_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (notifs_container.IsVisible)
            {
                DispatcherTimer timer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromSeconds(5)
                };

                timer.Tick += delegate (object s, EventArgs args)
                {
                    timer.Stop();
                    if (notifs_container.IsVisible) notifs_container.Visibility = Visibility.Hidden;
                };

                timer.Start();
            }
        }

        private void githubButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/captkirk88/monkeyspeak");
        }
    }
}