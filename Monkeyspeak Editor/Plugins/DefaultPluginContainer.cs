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
            plugins.AddRange(GetAllPlugins());
            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.Initialize();
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
            if (Assembly.GetEntryAssembly() != null)
            {
                // this detects the path from where the current EXE is being executed
                foreach (string asmFile in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "*.dll"))
                {
                    if (ReflectionHelper.TryLoad(asmFile, out Assembly asm))
                        foreach (var plugin in GetPluginsFromAssembly(asm)) yield return plugin;
                }
                foreach (var asmName in Assembly.GetEntryAssembly().GetReferencedAssemblies())
                {
                    var asm = Assembly.Load(asmName);
                    foreach (var plugin in GetPluginsFromAssembly(asm)) yield return plugin;
                }
            }
            else if (Assembly.GetExecutingAssembly() != null)
            {
                // this detects the path from where the current CODE is being executed
                foreach (string asmFile in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll"))
                {
                    if (ReflectionHelper.TryLoad(asmFile, out Assembly asm))
                        foreach (var plugin in GetPluginsFromAssembly(asm)) yield return plugin;
                }
                foreach (var asmName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                {
                    var asm = Assembly.Load(asmName);
                    foreach (var plugin in GetPluginsFromAssembly(asm)) yield return plugin;
                }
            }
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                // avoid all the Microsoft and System assemblies.  All assesmblies it is looking for should be in the local path
                if (asm.GlobalAssemblyCache) continue;

                foreach (var plugin in GetPluginsFromAssembly(asm)) yield return plugin;
            }
        }

        private static IEnumerable<IPlugin> GetPluginsFromAssembly(Assembly asm)
        {
            if (asm == null) yield break;

            foreach (var type in ReflectionHelper.GetAllTypesWithBaseClass<IPlugin>(asm))
            {
                if (ReflectionHelper.HasNoArgConstructor(type))
                    yield return (IPlugin)Activator.CreateInstance(type);
            }
        }
    }
}