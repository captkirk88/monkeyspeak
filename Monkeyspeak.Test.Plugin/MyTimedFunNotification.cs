using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monkeyspeak.Editor.Interfaces.Notifications;
using Monkeyspeak.Editor.Notifications;

namespace Monkeyspeak.Test.Plugin
{
    public class MyTimedFunNotification : TimedNotification
    {
        public MyTimedFunNotification(INotificationManager manager) : base(manager, TimeSpan.FromSeconds(3))
        {
        }
    }
}