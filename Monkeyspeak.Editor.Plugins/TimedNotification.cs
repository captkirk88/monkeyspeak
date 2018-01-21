using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Monkeyspeak.Editor.Interfaces.Notifications;
using Monkeyspeak.Editor.Plugins;

namespace Monkeyspeak.Editor.Notifications
{
    public abstract class TimedNotification : BaseNotification
    {
        private readonly INotificationManager manager;
        private DateTime end;
        private MahApps.Metro.Controls.MetroProgressBar ring;
        private StackPanel container;
        private UIElement content;

        protected TimedNotification(INotificationManager manager, TimeSpan timeToRemove)
        {
            end = DateTime.Now.Add(timeToRemove);
            ring = new MahApps.Metro.Controls.MetroProgressBar
            {
                Minimum = TimeSpan.Zero.TotalSeconds,
                Maximum = timeToRemove.TotalSeconds
            };
            container = new StackPanel();
            ring.Dispatcher.InvokeAsync(UpdateProgress);
            this.manager = manager;
        }

        public void SetContent(object content)
        {
            if (content is UIElement element)
                this.content = element;
            else this.content = new TextBlock { Text = content.ToString() };
        }

        public override object Content
        {
            get
            {
                container.Children.Clear();
                container.Children.Add(ring);
                container.Children.Add(content);
                ring.Dispatcher.InvokeAsync(UpdateProgress);
                return container;
            }
        }

        private void UpdateProgress()
        {
            var now = DateTime.Now;
            while (end < now)
            {
                now = DateTime.Now;
                ring.Value = (end - now).TotalSeconds;
            }
            manager.RemoveNotification(this);
        }
    }
}