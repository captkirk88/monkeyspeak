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
    public class MyFunConsoleCommand : IConsoleCommand
    {
        public string Command => "fun";

        public string Help => "lol";

        private bool ran = false;
        public bool CanInvoke => !ran;

        private Random rand = new Random();

        public void Invoke(IConsole console, IEditor editor, params string[] args)
        {
            ran = true;
            for (int i = 0; i <= 100; i++)
            {
                console.Write("lol", Color.FromRgb((byte)rand.Next(1, 255), (byte)rand.Next(1, 255), (byte)rand.Next(1, 255)));
            }
            console.WriteLine("", Colors.White);

            for (int i = 0; i <= 100; i++)
            {
                editor.AddLine("lol", Color.FromRgb((byte)rand.Next(1, 255), (byte)rand.Next(1, 255), (byte)rand.Next(1, 255)));
            }
        }
    }

    internal class HelpConsoleCommand : IConsoleCommand
    {
        public string Command => "help";

        public string Help => "Shows this";

        public bool CanInvoke => true;

        public void Invoke(IConsole console, IEditor editor, params string[] args)
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