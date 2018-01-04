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
    internal class StringNotification : INotification
    {
        private readonly TextBlock content;
        private Color foreground = Colors.White, background = Colors.Black;

        public StringNotification(string content)
        {
            this.content = new TextBlock
            {
                Text = content,
                TextWrapping = System.Windows.TextWrapping.WrapWithOverflow,
                Foreground = new SolidColorBrush(this.foreground),
                Background = new SolidColorBrush(this.background)
            };
        }

        public StringNotification(string content, Color foreground, Color background = default(Color))
        {
            this.content = new TextBlock
            {
                Text = content,
                TextWrapping = System.Windows.TextWrapping.WrapWithOverflow,
                Foreground = foreground != default(Color) ? new SolidColorBrush(foreground) : new SolidColorBrush(this.foreground),
                Background = background != default(Color) ? new SolidColorBrush(background) : new SolidColorBrush(this.background),
                Width = double.NaN,
                Height = double.NaN
            };
            if (foreground != default(Color))
                this.foreground = foreground;
            if (background != default(Color))
                this.background = background;
        }

        public object Content => content;
    }
}