using System.Windows;
using System.Windows.Controls;

namespace Monkeyspeak.Editor.Notifications.Controls
{
    /// <summary>
    /// Interaction logic for NotificationPanel.xaml
    /// </summary>
    public partial class NotificationPanel : UserControl
    {
        private INotification notif;

        public NotificationPanel()
        {
            InitializeComponent();
            this.Unloaded += NotificationPanel_Unloaded;
            NotificationManager.Removed += NotificationManager_Removed;
        }

        public NotificationPanel(INotification notif) : this()
        {
            this.notif = notif;
            TextBlock.Text = notif.Content.ToString();
        }

        private void NotificationPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            NotificationManager.Removed -= NotificationManager_Removed;
        }

        private void NotificationManager_Removed(INotification notif)
        {
            if (this.notif == notif)
                ((ListView)this.Parent)?.Items?.Remove(this);
        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            NotificationManager.Remove(notif);
        }
    }
}