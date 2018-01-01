using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Notifications
{
    public abstract class BaseNotification : INotification
    {
        public abstract object Content { get; }

        public virtual Color ForegroundColor
        {
            get
            {
                var theme = ThemeManager.DetectAppStyle().Item1;
                if (theme.Name == "Dark")
                    return Colors.White;
                if (theme.Name == "Light")
                    return Colors.Black;
                return Colors.White;
            }
        }

        public virtual Color BackgroundColor
        {
            get
            {
                var theme = ThemeManager.DetectAppStyle().Item1;
                if (theme.Name == "Dark")
                    return Colors.Black;
                if (theme.Name == "Light")
                    return Colors.White;
                return Colors.White;
            }
        }
    }
}