using System.Collections.Generic;

namespace Monkeyspeak.Editor.Interfaces.Plugins
{
    public interface IPluginContainer
    {
        void Initialize();

        void Unload();

        bool HasPlugin<T>() where T : IPlugin;

        T GetPlugin<T>() where T : IPlugin;
    }
}