using Monkeyspeak.Editor.Notifications;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Logging
{
    public class NotificationPanelLogOutput : ILogOutput
    {
        private readonly Level level;

        public NotificationPanelLogOutput(Level level)
        {
            this.level = level;
        }

        public void Log(LogMessage logMsg)
        {
            Color color = Colors.White;
            if (logMsg.Level != level) return;
            NotificationWithIcon.IconKind icon = NotificationWithIcon.IconKind.Info;
            switch (logMsg.Level)
            {
                case Level.Error:
                    color = Colors.Red;
                    icon = NotificationWithIcon.IconKind.Hazard;
                    break;

                case Level.Warning:
                    color = Colors.Yellow;
                    icon = NotificationWithIcon.IconKind.Warning;
                    break;

                case Level.Debug:
                    color = Colors.Silver;
                    icon = NotificationWithIcon.IconKind.Bug;
                    break;

                default:
                    icon = NotificationWithIcon.IconKind.Info;
                    break;
            }
            var sb = new StringBuilder();
            sb.AppendLine(logMsg.TimeStamp.ToString("hh:mm:fff"));
            sb.AppendLine(logMsg.message);
            try
            {
                NotificationManager.Instance.AddNotification(new NotificationWithIcon(icon, sb.ToString()));
            }
            catch { }
        }
    }
}