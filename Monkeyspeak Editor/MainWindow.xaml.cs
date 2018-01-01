using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Monkeyspeak.Editor.Logging;
using Monkeyspeak.Editor.Notifications;
using Monkeyspeak.Editor.Notifications.Controls;
using Monkeyspeak.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
            NotificationManager.Added += notif => notifs_list.Items.Add(new NotificationPanel(notif));

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

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            //((App)Application.Current).SetColor(AppColor.Brown);
            NotificationManager.Add(new StringNotification("Hello World"));
            Task.Run(async () =>
            {
                await Task.Run(() =>
                {
                    Parallel.For(0, 1000, i =>
                    {
                        Logger.Info(i);
                    });
                });
            });
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
                notifs_container.IsOpen = !notifs_container.IsOpen;
            else
                if (notifs_container.IsOpen) notifs_container.IsOpen = false;
        }

        private void Notifications_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                NotificationManager.Clear();
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}