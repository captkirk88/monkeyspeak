using Monkeyspeak.Editor.Interfaces.Plugins;
using Monkeyspeak.Logging;
using Monkeyspeak.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Plugins
{
    internal static class Plugins
    {
        private static List<IPlugin> plugins;

        static Plugins()
        {
            plugins = new List<IPlugin>();
            plugins.AddRange(GetAllPlugins());
        }

        public static IReadOnlyCollection<IPlugin> All => plugins.AsReadOnly();

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
                            Logger.Debug($"Registering console command {type.Name}", null);
                            yield return (IPlugin)plugin;
                        }
                    }
                }
            }
        }
    }
}