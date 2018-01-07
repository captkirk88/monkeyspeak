using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Interfaces.Console
{
    public interface IConsole
    {
        void Write(string content, Color color = default(Color));

        void WriteLine(string content, Color color = default(Color));
    }
}