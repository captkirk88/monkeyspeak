using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Monkeyspeak.Editor.Interfaces.Notifications;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
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
            NotificationManager.Instance.Removed += Notification_Removed;
            DataContext = this;
        }

        private void Notification_Removed(INotification notif)
        {
            if (notif == this.notif)
                Delete();
        }

        public NotificationPanel(INotification notif) : this()
        {
            if (notif != null && notif.Content != null)
            {
                this.notif = notif;
                if (notif is ICriticalNotification)
                {
                    DismissButton.Visibility = Visibility.Hidden;
                    Container.Children.Remove(DismissButton);
                    UpdateLayout();
                }
            }
        }

        public void Delete()
        {
            (Parent as ListView).Items.Remove(this);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void NotificationPanel_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            NotificationManager.Instance.RemoveNotification(notif);
        }

        private void ContentContainer_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async void OnMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            return;
            var stackPanel = new StackPanel();
            var content = XamlReader.Parse(XamlWriter.Save(ContentContainer.Content));
            var contentElement = content as UIElement ?? new TextBlock { Text = content.ToString() };
        }
    }
}