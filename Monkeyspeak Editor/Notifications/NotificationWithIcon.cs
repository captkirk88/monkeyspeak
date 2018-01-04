using MahApps.Metro;
using Monkeyspeak.Editor.Interfaces.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Notifications
{
    public class NotificationWithIcon : INotification
    {
        public enum IconKind
        {
            Hazard,
            Info,
            Warning,
            Bug
        }

        private readonly string message;
        private StackPanel content;

        public object Content => content;

        public Color ForegroundColor
        {
            get
            {
                var theme = ThemeManager.DetectAppStyle().Item1;
                if (theme.Name == "Dark")
                    return Colors.White;
                else if (theme.Name == "Light")
                    return Colors.Black;
                return Colors.Green;
            }
        }

        public Color BackgroundColor
        {
            get;
        }

        public NotificationWithIcon(IconKind kind, string message)
        {
            this.message = message;
            MahApps.Metro.IconPacks.PackIconModernKind result;
            Brush foreground;
            switch (kind)
            {
                case IconKind.Hazard:
                    result = MahApps.Metro.IconPacks.PackIconModernKind.TransitHazard;
                    foreground = Brushes.Red;
                    break;

                case IconKind.Info:
                    result = MahApps.Metro.IconPacks.PackIconModernKind.Information;
                    foreground = Brushes.White;
                    break;

                case IconKind.Warning:
                    result = MahApps.Metro.IconPacks.PackIconModernKind.Warning;
                    foreground = Brushes.Yellow;
                    break;

                case IconKind.Bug:
                    result = MahApps.Metro.IconPacks.PackIconModernKind.Bug;
                    foreground = Brushes.Green;
                    break;

                default:
                    result = MahApps.Metro.IconPacks.PackIconModernKind.Information;
                    foreground = Brushes.White;
                    break;
            }
            var img = new Button()
            {
                Content = new MahApps.Metro.IconPacks.PackIconModern
                {
                    Kind = result,
                    Foreground = foreground
                },
                IsHitTestVisible = false,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new System.Windows.Thickness(0),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
            };

            var tb = new TextBlock()
            {
                Text = message,
                TextTrimming = System.Windows.TextTrimming.WordEllipsis,
                Width = double.NaN,
                ToolTip = message
            };
            content = new StackPanel();
            content.Children.Add(img);
            content.Children.Add(tb);
        }
    }
}