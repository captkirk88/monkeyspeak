using Monkeyspeak.Editor.Interfaces.Plugins;

namespace Monkeyspeak.Editor.Plugins
{
    public abstract class Plugin : IPlugin
    {
        public virtual string Name { get => GetType().Name; }

        public abstract void Execute(IEditor editor);

        public abstract void Initialize(IPluginContainer pluginContainer);
    }
}