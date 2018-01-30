using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
        private ProgressBar countdown;
        private DockPanel container;
        private UIElement content;
        private Timer timer;

        public TimedNotification(INotificationManager manager, TimeSpan timeToRemove)
        {
            if (timeToRemove.TotalSeconds < 1d) timeToRemove = TimeSpan.FromSeconds(1);
            end = DateTime.Now.Add(timeToRemove);
            countdown = new ProgressBar
            {
                Maximum = (end - DateTime.Now).TotalSeconds,
                Minimum = 0d
            };
            container = new DockPanel();
            timer = new Timer(200)
            {
                AutoReset = true
            };
            timer.Elapsed += (sender, e) => container.Dispatcher.Invoke(UpdateProgress);
            timer.Start();
            this.manager = manager;
        }

        public virtual object SetContent()
        {
            return null;
        }

        public override object Content
        {
            get
            {
                container.Children.Clear();
                DockPanel.SetDock(countdown, Dock.Top);
                container.Children.Add(countdown);
                var content = SetContent();
                if (content != null)
                {
                    var element = content as UIElement;
                    if (element == null)
                    {
                        element = new TextBlock { Text = content.ToString() };
                    }
                    DockPanel.SetDock(element, Dock.Bottom);
                    container.Children.Add(element);
                }
                return container;
            }
        }

        private void UpdateProgress()
        {
            var now = DateTime.Now;
            if (end > now)
            {
                countdown.Value = (end - now).TotalSeconds;
            }
            else
            {
                manager.RemoveNotification(this);
                timer.Stop();
                timer.Dispose();
            }
        }
    }
}