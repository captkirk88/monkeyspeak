using Monkeyspeak.Editor.Interfaces.Notifications;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Monkeyspeak.Editor.Notifications
{
    public class NotificationManager : INotificationManager
    {
        public static NotificationManager Instance = new NotificationManager();

        private ConcurrentQueue<INotification> notifs = new ConcurrentQueue<INotification>();

        public event Action<INotification> Added, Removed;

        public int Count => notifs.Count;

        public void AddNotification(INotification notif)
        {
            notifs.Enqueue(notif);
            Added?.Invoke(notif);
        }

        public void RemoveNotification(INotification notif)
        {
            if (notifs.TryDequeue(out INotification existing))
                Removed?.Invoke(existing);
        }

        public IReadOnlyCollection<INotification> All => notifs;

        public void Clear()
        {
            while (notifs.Count > 0)
            {
                if (notifs.TryDequeue(out INotification notif))
                    Removed?.Invoke(notif);
            }
        }
    }
}