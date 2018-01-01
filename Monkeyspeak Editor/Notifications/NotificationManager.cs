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
            notifs.RemoveAll(notif =>
            {
                Removed?.Invoke(notif);
                return true;
            });
        }
    }
}