using Monkeyspeak.Editor.Interfaces.Plugins;
using Monkeyspeak.Logging;
using Monkeyspeak.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Plugins
{
    internal static class Plugins
    {
        private static ObservableCollection<IPlugin> plugins;

        static Plugins()
        {
            plugins = new ObservableCollection<IPlugin>();
            foreach (var plugin in GetAllPlugins()) plugins.Add(plugin);
        }

        public static ObservableCollection<IPlugin> All => plugins;

        public static void Add(IPlugin plugin)
        {
            plugins.Add(plugin);
        }

        public static void Remove(IPlugin plugin)
        {
            if (plugin != null)
                plugins.Remove(plugin);
        }

        private static IEnumerable<IPlugin> GetAllPlugins()
        {
            foreach (var asm in ReflectionHelper.GetAllAssemblies())
            {
                foreach (var type in ReflectionHelper.GetAllTypesWithInterface<IPlugin>(asm))
                {
                    if (ReflectionHelper.HasNoArgConstructor(type))
                    {
                        if (ReflectionHelper.TryCreate(type, out var plugin))
                        {
                            Logger.Debug($"Registering plugin {type.Name}", null);
                            yield return (IPlugin)plugin;
                        }
                    }
                }
            }
        }
    }
}