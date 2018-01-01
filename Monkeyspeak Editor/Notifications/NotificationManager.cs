using System;
using System.Collections.Generic;

namespace Monkeyspeak.Editor.Notifications
{
    public static class NotificationManager
    {
        private static List<INotification> notifs = new List<INotification>();

        public static event Action<INotification> Added, Removed;

        public static int Count => notifs.Count;

        public static void Add(INotification notif)
        {
            notifs.Add(notif);
            Added?.Invoke(notif);
        }

        public static void Remove(INotification notif)
        {
            notifs.Remove(notif);
            Removed?.Invoke(notif);
        }

        public static void Clear()
        {
            for (int i = 0; i <= notifs.Count - 1; i++)
            {
                var notif = notifs[i];
                notifs.RemoveAt(i);
                Removed?.Invoke(notif);
            }
        }
    }
}