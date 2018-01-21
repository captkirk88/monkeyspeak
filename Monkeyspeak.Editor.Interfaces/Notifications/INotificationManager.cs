using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Interfaces.Notifications
{
    public interface INotificationManager
    {
        void AddNotification(INotification notification);

        void RemoveNotification(INotification notification);
    }
}