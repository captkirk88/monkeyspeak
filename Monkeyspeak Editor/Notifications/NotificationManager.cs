using Monkeyspeak.Collections;
using Monkeyspeak.Editor.Interfaces.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Monkeyspeak.Editor.Notifications
{
    public class NotificationManager : INotificationManager
    {
        public static NotificationManager Instance = new NotificationManager();

        private ConcurrentList<INotification> notifs = new ConcurrentList<INotification>();

        public event Action<INotification> Added, Removed;

        public int Count => notifs.Count(n => !(n is ICriticalNotification));

        public bool HasCriticalNotifications => notifs.Count(n => n is ICriticalNotification) > 0;

        public void AddNotification(INotification notif)
        {
            notifs.Add(notif);
            Added?.Invoke(notif);
        }

        public void RemoveNotification(INotification notif)
        {
            if (notif is ICriticalNotification) return;
            if (notifs.Remove(notif))
                Removed?.Invoke(notif);
        }

        public IReadOnlyCollection<INotification> All => notifs.AsReadOnly();

        public void Clear()
        {
            foreach (var notif in notifs)
            {
                RemoveNotification(notif);
            }
        }
    }
}