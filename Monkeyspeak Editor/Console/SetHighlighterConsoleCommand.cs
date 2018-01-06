using ICSharpCode.AvalonEdit.Highlighting;
using Monkeyspeak.Editor.Interfaces.Console;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Console
{
    internal class SetHighlighterConsoleCommand : IConsoleCommand
    {
        public string Command => "syntax";

        public string Help => @"Usage:
    syntax list - shows all syntax highlighter definition
    syntax set <name> - sets the syntax highlighter to the <name>
";

        public void Invoke(IConsole console, IEditor editor, params string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "list")
                {
                    var sb = new StringBuilder();
                    foreach (var def in HighlightingManager.Instance.HighlightingDefinitions)
                        sb.AppendLine(def.Name);
                    console.WriteLine(sb.ToString(), Colors.White);
                }
                else if (args[0] == "set")
                {
                    string last = editor.HighlighterLanguage, current;
                    editor.HighlighterLanguage = current = args[0];
                    console.WriteLine($"Syntax highlighter changed from {last} to {current}", Colors.Yellow);
                }
            }
        }
    }
}