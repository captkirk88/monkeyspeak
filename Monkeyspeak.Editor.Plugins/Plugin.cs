using Monkeyspeak.Editor.Interfaces.Notifications;
using Monkeyspeak.Editor.Interfaces.Plugins;

namespace Monkeyspeak.Editor.Plugins
{
    public abstract class Plugin : IPlugin
    {
        protected Plugin()
        {
            Enabled = true;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name { get => GetType().Name; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Plugin"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public virtual bool Enabled { get; set; }

        protected INotificationManager NotificationManager { get; }

        public void AddNotification(INotification notif)
        {
            NotificationManager.AddNotification(notif);
        }

        /// <summary>
        /// Executes on the current Editor that has focus.
        /// </summary>
        /// <param name="editor">The editor.</param>
        public abstract void Execute(IEditor editor);

        /// <summary>
        /// Initializes the specified plugin container.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        public abstract void Unload();
    }
}