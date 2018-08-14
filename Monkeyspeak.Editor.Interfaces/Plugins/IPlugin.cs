﻿using Monkeyspeak.Editor.Interfaces.Notifications;
using Monkeyspeak.Editor.Interfaces.Plugins;

namespace Monkeyspeak.Editor.Interfaces.Plugins
{
    public interface IPlugin
    {
        /// <summary>
        /// Initializes this plugin.
        /// </summary>
        /// <param name="notificationManager">
        /// The notification manager. Add your notifications or store the notificationManager
        /// somewhere to use later on.
        /// </param>
        void Initialize(INotificationManager notificationManager);

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
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
    }
}