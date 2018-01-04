using Monkeyspeak.Editor.Interfaces.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Notifications
{
    public class WelcomeNotification : INotification
    {
        public object Content
        {
            get
            {
                var stackPanel = new StackPanel();
                var tb = new TextBlock
                {
                    Text = "Thank you for using the Monkeyspeak Editor!",
                    TextWrapping = System.Windows.TextWrapping.Wrap,
                };

                stackPanel.Children.Add(tb);
                return stackPanel;
            }
        }
    }
}