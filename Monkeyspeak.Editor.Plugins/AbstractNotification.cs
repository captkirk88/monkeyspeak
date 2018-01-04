using Monkeyspeak.Editor.Interfaces.Notifications;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Plugins
{
    public abstract class AbstractNotification : INotification
    {
        public abstract object Content { get; }

        public virtual Color ForegroundColor { get; set; }

        public virtual Color BackgroundColor { get; set; }
    }
}