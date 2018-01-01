using Monkeyspeak.Editor.Interfaces.Plugins;

namespace Monkeyspeak.Editor.Interfaces.Plugins
{
    public interface IPlugin
    {
        void Initialize(IPluginContainer pluginContainer);

        /// <summary>
        /// Executes on the current Editor that has focus.
        /// </summary>
        /// <param name="editor">The editor.</param>
        void Execute(IEditor editor);

        void Unload();

        string Name { get; }
    }
}