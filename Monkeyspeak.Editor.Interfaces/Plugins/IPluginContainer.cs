using System.Collections.Generic;

namespace Monkeyspeak.Editor.Interfaces.Plugins
{
    public interface IPluginContainer
    {
        void Initialize();

        void Execute(IEditor editor);

        void Unload();

        ICollection<IPlugin> Plugins { get; }
    }
}