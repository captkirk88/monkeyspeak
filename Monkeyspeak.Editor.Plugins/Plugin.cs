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

        /// <summary>
        /// Initializes the specified plugin container.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called when [editor selection changed].
        /// </summary>
        /// <param name="editor">The editor.</param>
        public abstract void OnEditorSelectionChanged(IEditor editor);

        /// <summary>
        /// Called when [editor text changed].
        /// </summary>
        /// <param name="editor">The editor.</param>
        public abstract void OnEditorTextChanged(IEditor editor);

        /// <summary>
        /// Called when [editor save completed].
        /// </summary>
        /// <param name="editor">The editor.</param>
        public abstract void OnEditorSaveCompleted(IEditor editor);

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        public abstract void Unload();
    }
}