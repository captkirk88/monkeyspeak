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
        private ListView notifs_list;

        public MainWindow()
        {
            InitializeComponent();
            console = new ConsoleWindow();
            Logger.LogOutput = new MultiLogOutput(new ConsoleWindowLogOutput(console), new NotificationPanelLogOutput());
            notifs_list = new ListView();
            notifs_container.Child = notifs_list;
            NotificationManager.Added += notif => notif_badge.Badge = NotificationManager.Count;
            NotificationManager.Removed += notif => notif_badge.Badge = NotificationManager.Count;
            NotificationManager.Added += notif => notifs_list.Items.Add(new NotificationPanel(notif));

            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i <= 1000; i++)
            {
                Logger.Info(i);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            console.Close();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            //((App)Application.Current).SetColor(AppColor.Brown);
            NotificationManager.Add(new StringNotification("Hello World"));
            for (int i = 0; i <= 1000; i++) Logger.Info(i);
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

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var prepare = new PrepareDialog(this, "Preparing..", "Gathering truck loads of monkeys.");
            prepare.DoWork += () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    prepare.Progress = i * 10;
                    Thread.Sleep(100);
                    if (prepare.CancellationPending) break;
                }
            };
            //prepare.ShowDialogExternally();
        }
    }

    internal class PrepareDialog : BaseMetroDialog
    {
        public event Action Finished, DoWork;

        private TextBlock text;
        private CancellationTokenSource cts;

        public PrepareDialog(string title, string message)
        {
            Title = title;
            var content = new StackPanel();
            text = new TextBlock
            {
                Text = message
            };
            content.Children.Add(text);
            Content = content;
            Loaded += PrepareDialog_Loaded;
            cts = new CancellationTokenSource();
        }

        public PrepareDialog(MetroWindow owningWindow, string title, string message, MetroDialogSettings settings = null) : base(owningWindow, settings)
        {
            Title = title;
            var content = new StackPanel();
            text = new TextBlock
            {
                Text = message,
                Padding = new Thickness(10d)
            };

            content.Children.Add(text);
            Content = content;
            Loaded += PrepareDialog_Loaded;
            cts = new CancellationTokenSource();
        }

        public string Text { get => text.Text; set => text.Text = value; }

        public int Progress { get; set; }

        public bool CancellationPending { get => cts.IsCancellationRequested; }

        public void Abort()
        {
            cts.Cancel();
            Finished?.Invoke();
        }

        private void PrepareDialog_Loaded(object sender, RoutedEventArgs e)
        {
            OnWork();
        }

        protected void OnWork()
        {
            Task.Run(() =>
            {
                while (this.Progress < 99)
                {
                    if (cts.IsCancellationRequested) Progress = 100;
                    DoWork?.Invoke();
                }
            }, cts.Token).ContinueWith(task =>
            {
                Finished?.Invoke();
                if (OwningWindow != null)
                    DialogManager.HideMetroDialogAsync(OwningWindow, this);
                else RequestCloseAsync();
            });
        }
    }
}