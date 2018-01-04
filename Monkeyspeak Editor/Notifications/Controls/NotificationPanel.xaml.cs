using MahApps.Metro;
using Monkeyspeak.Editor.Interfaces.Notifications;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Notifications.Controls
{
    /// <summary>
    /// Interaction logic for NotificationPanel.xaml
    /// </summary>
    public partial class NotificationPanel : UserControl
    {
        private INotification notif;

        public INotification Notification { get => notif; set => notif = value; }

        public NotificationPanel()
        {
            InitializeComponent();
            Unloaded += NotificationPanel_Unloaded;
            NotificationManager.Instance.Removed += NotificationManager_Removed;
            DataContext = this;
        }

        public NotificationPanel(INotification notif) : this()
        {
            this.notif = notif;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void NotificationPanel_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void NotificationManager_Removed(INotification notif)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (this.notif == notif)
                {
                    ((ListView)this.Parent)?.Items?.Remove(this);
                    NotificationManager.Instance.Removed -= NotificationManager_Removed;
                }
            });
        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            NotificationManager.Instance.RemoveNotification(notif);
        }

        private void ContentContainer_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}