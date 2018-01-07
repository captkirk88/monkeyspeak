using Monkeyspeak.Editor.Interfaces.Console;
using Monkeyspeak.Editor.Notifications;

namespace Monkeyspeak.Editor.Console
{
    public class NotifyConsoleCommand : IConsoleCommand
    {
        public string Command => "notify";

        public string Help => "Sends a notification";

        public bool CanInvoke => true;

        public void Invoke(IConsole console, IEditor editor, params string[] args)
        {
            var message = string.Join(" ", args);
            if (!string.IsNullOrWhiteSpace(message))
                NotificationManager.Instance.AddNotification(new StringNotification(message));
        }
    }
}