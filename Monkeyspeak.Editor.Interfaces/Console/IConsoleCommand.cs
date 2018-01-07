using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Interfaces.Console
{
    public interface IConsoleCommand
    {
        string Command { get; }

        string Help { get; }

        bool CanInvoke { get; }

        void Invoke(IConsole console, IEditor editor, params string[] args);
    }
}