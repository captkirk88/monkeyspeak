using Monkeyspeak.Editor.Interfaces.Notifications;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Plugins
{
    public abstract class BaseNotification : INotification
    {
        protected BaseNotification()
        {
        }

        public abstract object Content { get; }
    }
}