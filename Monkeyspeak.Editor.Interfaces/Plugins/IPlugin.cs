using Monkeyspeak.Editor.Interfaces.Notifications;
using Monkeyspeak.Editor.Interfaces.Plugins;

namespace Monkeyspeak.Editor.Interfaces.Plugins
{
    public interface IPlugin
    {
        /// <summary>
        /// Initializes this plugin.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Add your notifications in this method
        /// </summary>
        /// <param name="notificationManager">The notification manager.</param>
        void AddNotifications(INotificationManager notificationManager);

        /// <summary>
        /// Called when [editor text changed].
        /// </summary>
        /// <param name="editor">The editor.</param>
        void OnEditorTextChanged(IEditor editor);

        /// <summary>
        /// Called when [editor selection changed].
        /// </summary>
        /// <param name="editor">The editor.</param>
        void OnEditorSelectionChanged(IEditor editor);

        /// <summary>
        /// Called when [editor save completed].
        /// </summary>
        /// <param name="editor">The editor.</param>
        void OnEditorSaveCompleted(IEditor editor);

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