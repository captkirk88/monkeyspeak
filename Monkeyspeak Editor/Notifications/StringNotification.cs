using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Notifications
{
    internal class StringNotification : INotification
    {
        private readonly string content;
        private Color foreground = Colors.White, background = Colors.Black;

        public StringNotification(string content)
        {
            this.content = content;
        }

        public StringNotification(string content, Color foreground, Color background = default(Color))
        {
            this.content = content;
            this.foreground = foreground;
            if (background != default(Color))
                this.background = background;
        }

        public object Content => content;

        public Color ForegroundColor { get => foreground; set => foreground = value; }

        public Color BackgroundColor { get => background; set => background = value; }
    }
}