using Monkeyspeak.Editor.Interfaces.Plugins;

namespace Monkeyspeak.Editor.Plugins
{
    public abstract class Plugin : IPlugin
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name { get => GetType().Name; }

        /// <summary>
        /// Executes on the current Editor that has focus.
        /// </summary>
        /// <param name="editor">The editor.</param>
        public abstract void Execute(IEditor editor);

        /// <summary>
        /// Initializes the specified plugin container.
        /// </summary>
        /// <param name="pluginContainer">The plugin container.</param>
        public abstract void Initialize(IPluginContainer pluginContainer);

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        public abstract void Unload();
    }
}