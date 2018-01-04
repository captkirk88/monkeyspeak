using Monkeyspeak.Editor.Interfaces.Notifications;
using Monkeyspeak.Editor.Interfaces.Plugins;

namespace Monkeyspeak.Editor.Interfaces.Plugins
{
    public interface IPlugin
    {
        void Initialize();

        /// <summary>
        /// Executes on the current Editor that has focus.
        /// </summary>
        /// <param name="editor">The editor.</param>
        void Execute(IEditor editor);

        void AddNotification(INotification notif);

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        void Unload();

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Plugin"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }
    }
}