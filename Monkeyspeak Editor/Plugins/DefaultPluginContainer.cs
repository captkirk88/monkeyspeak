using Monkeyspeak.Editor.Interfaces.Plugins;
using Monkeyspeak.Logging;
using Monkeyspeak.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Plugins
{
    internal class DefaultPluginContainer : IPluginContainer
    {
        public void Initialize()
        {
            foreach (var plugin in Plugins.All)
            {
                try
                {
                    plugin.Initialize();
                }
                catch (Exception ex)
                {
                    ex.Log<IPlugin>();
                }
            }
        }

        public T GetPlugin<T>() where T : IPlugin
        {
            return Plugins.All.OfType<T>().FirstOrDefault();
        }

        public bool HasPlugin<T>() where T : IPlugin
        {
            return Plugins.All.OfType<T>().Count() > 0;
        }

        public void Unload()
        {
            foreach (var plugin in Plugins.All)
            {
                try
                {
                    plugin.Unload();
                }
                catch (Exception ex)
                {
                    Logger.Error<DefaultPluginContainer>(ex);
                }
            }
        }
    }
}