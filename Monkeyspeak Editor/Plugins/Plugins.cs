using Monkeyspeak.Editor.Interfaces.Plugins;
using Monkeyspeak.Editor.Notifications;
using Monkeyspeak.Logging;
using Monkeyspeak.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Monkeyspeak.Editor.Plugins
{
    internal static class PluginsManager
    {
        private static ObservableCollection<IPlugin> plugins;

        static PluginsManager()
        {
            plugins = new ObservableCollection<IPlugin>();
            foreach (var plugin in GetAllPlugins()) plugins.Add(plugin);
        }

        public static ObservableCollection<IPlugin> All => plugins;

        public static bool AllEnabled
        {
            get
            {
                var result = false;
                foreach (var plugin in plugins)
                    result &= plugin.Enabled;
                return result;
            }
            set
            {
                foreach (var plugin in plugins) plugin.Enabled = value;
            }
        }

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
                        if (ReflectionHelper.TryCreate<IPlugin>(type, out var plugin))
                        {
                            Logger.Debug($"Registering plugin {type.Name}");
                            yield return plugin;
                        }
                        else
                        {
                            Logger.Error($"Failed to register plugin {type.Name}");
                        }
                }
            }
        }

        public static void Initialize()
        {
            foreach (var plugin in plugins)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        plugin.Initialize();
                        plugin.AddNotifications(NotificationManager.Instance);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error<Plugin>($"Failed to initialize {plugin.Name}");
                        ex.Log<Plugin>();
                    }
                });
            }
        }

        public static void OnEditorSaveCompleted(IEditor editor)
        {
            foreach (var plugin in plugins)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        plugin.OnEditorSaveCompleted(editor);
                    }
                    catch (Exception ex)
                    {
                        ex.Log<Plugin>();
                    }
                });
            }
        }

        public static void OnEditorSelectionChanged(IEditor editor)
        {
            foreach (var plugin in plugins)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        plugin.OnEditorSelectionChanged(editor);
                    }
                    catch (Exception ex)
                    {
                        ex.Log<Plugin>();
                    }
                });
            }
        }

        public static void OnEditorTextChanged(IEditor editor)
        {
            foreach (var plugin in plugins)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            plugin.OnEditorTextChanged(editor);
                        }
                        catch (Exception ex)
                        {
                            ex.Log<Plugin>();
                        }
                    });
            }
        }

        internal static void Unload()
        {
            foreach (var plugin in plugins)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        plugin.Unload();
                    }
                    catch (Exception ex)
                    {
                        ex.Log<Plugin>();
                    }
                });
            }
        }
    }
}