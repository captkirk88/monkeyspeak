using Monkeyspeak.Editor.Interfaces.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Console
{
    internal class PluginsConsoleCommand : IConsoleCommand
    {
        public string Command => "plugins";

        public string Help => "Lists all active plugins";

        public bool CanInvoke => true;

        public void Invoke(IConsole console, IEditor editor, params string[] args)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Plugins: {Plugins.Plugins.All.Count}");
            foreach (var plugin in Plugins.Plugins.All)
            {
                sb.AppendLine(plugin.Name ?? plugin.GetType().Name);
            }
            console.Write(sb.ToString(), Colors.Teal);
        }
    }
}