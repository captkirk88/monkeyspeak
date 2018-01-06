using Monkeyspeak.Editor.Controls;
using Monkeyspeak.Editor.Interfaces.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Console
{
    internal class HelpConsoleCommand : IConsoleCommand
    {
        public string Command => "help";

        public string Help => "Shows this";

        public void Invoke(IConsole console, params string[] args)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Commands:");
            foreach (var command in ((ConsoleWindow)console).commands)
            {
                sb.AppendLine($"{command.Command} - {command.Help ?? "No information"}");
            }
            console.Write(sb.ToString(), Colors.Yellow);
        }
    }
}