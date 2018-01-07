using Monkeyspeak.Editor;
using Monkeyspeak.Editor.Interfaces.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Test.Plugin
{
    public class MyTestConsoleCommand : IConsoleCommand
    {
        public string Command => "test";

        public string Help => "Just a test";

        public bool CanInvoke => true;

        public void Invoke(IConsole console, IEditor editor, params string[] args)
        {
            console.WriteLine("Just a test");
        }
    }
}