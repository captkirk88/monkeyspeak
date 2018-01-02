using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Monkeyspeak.Editor.Commands
{
    public static class MonkeyspeakCommands
    {
        public static NewEditorCommand New = new NewEditorCommand();

        public static OpenFileCommand Open = new OpenFileCommand();

        public static SaveCommand Save = new SaveCommand();

        public static SaveAllCommand SaveAll = new SaveAllCommand();

        public static CloseCurrentEditorCommand Close = new CloseCurrentEditorCommand();

        public static ExitCommand Exit = new ExitCommand();
    }
}