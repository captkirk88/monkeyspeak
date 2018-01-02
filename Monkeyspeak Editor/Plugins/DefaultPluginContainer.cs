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
        private List<IPlugin> plugins = new List<IPlugin>();
        public ICollection<IPlugin> Plugins { get => plugins; }

        public void Initialize()
        {
            foreach (var plugin in GetAllPlugins())
            {
                plugins.Add(plugin);
                try
                {
                    plugin.Initialize();
                    Logger.Debug<DefaultPluginContainer>($"Added plugin {plugin.Name}");
                }
                catch (Exception ex)
                {
                    Logger.Error<DefaultPluginContainer>(ex);
                }
            }
        }

        public void Execute(IEditor editor)
        {
            foreach (var plugin in plugins)
            {
                try
                {
                    if (plugin.Enabled)
                        plugin.Execute(editor);
                }
                catch (Exception ex)
                {
                    Logger.Error<DefaultPluginContainer>(ex);
                }
            }
        }

        public void Unload()
        {
            foreach (var plugin in plugins)
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

        private static IEnumerable<IPlugin> GetAllPlugins()
        {
            foreach (var asm in ReflectionHelper.GetAllAssemblies())
            {
                foreach (var plugin in GetPluginsFromAssembly(asm)) yield return plugin;
            }
        }

        private static IEnumerable<IPlugin> GetPluginsFromAssembly(Assembly asm)
        {
            if (asm == null) yield break;
            foreach (var type in ReflectionHelper.GetAllTypesWithInterface<IPlugin>(asm))
            {
                Logger.Debug<DefaultPluginContainer>($"Found plugin {type.Name}");
                if (ReflectionHelper.HasNoArgConstructor(type))
                    yield return (IPlugin)Activator.CreateInstance(type);
            }
        }
    }
}