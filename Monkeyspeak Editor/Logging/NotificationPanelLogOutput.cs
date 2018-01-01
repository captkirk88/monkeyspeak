using Monkeyspeak.Editor.Notifications;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Logging
{
    public class NotificationPanelLogOutput : ILogOutput
    {
        public void Log(LogMessage logMsg)
        {
            Color color = Colors.White;
            switch (logMsg.Level)
            {
                case Level.Error:
                    color = Colors.Red;
                    break;

                case Level.Warning:
                    color = Colors.Yellow;
                    break;

                case Level.Debug:
                    color = Colors.Silver;
                    break;
            }
            var sb = new StringBuilder();
            sb.AppendLine(logMsg.TimeStamp.ToString("hh:mm:fff"));
            sb.AppendLine(logMsg.message);
            Notifications.NotificationManager.Add(new StringNotification(sb.ToString(), color));
        }
    }
}