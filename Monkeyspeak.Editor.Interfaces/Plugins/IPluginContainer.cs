using System.Collections.Generic;

namespace Monkeyspeak.Editor.Interfaces.Plugins
{
    public interface IPluginContainer
    {
        void Initialize();

        void Unload();

        ICollection<IPlugin> Plugins { get; }
    }
}