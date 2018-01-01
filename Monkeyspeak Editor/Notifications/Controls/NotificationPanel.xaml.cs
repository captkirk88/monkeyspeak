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
        }

        public NotificationPanel(INotification notif) : this()
        {
            this.notif = notif;
            Text.Text = notif.Content.ToString();
        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            ((ListView)this.Parent).Items.Remove(this);
            NotificationManager.Remove(notif);
        }
    }
}