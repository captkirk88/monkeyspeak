using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Monkeyspeak.Editor.Notifications
{
    public static class NotificationManager
    {
        private static ConcurrentQueue<INotification> notifs = new ConcurrentQueue<INotification>();

        public static event Action<INotification> Added, Removed;

        public static int Count => notifs.Count;

        public static void Add(INotification notif)
        {
            notifs.Enqueue(notif);
            Added?.Invoke(notif);
        }

        public static void Remove(INotification notif)
        {
            if (notifs.TryDequeue(out INotification existing))
                Removed?.Invoke(existing);
        }

        public static IReadOnlyCollection<INotification> All => notifs;

        public static void Clear()
        {
            while (notifs.Count > 0)
            {
                if (notifs.TryDequeue(out INotification notif))
                    Removed?.Invoke(notif);
            }
        }
    }
}