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

        public static NavigateToDocumentPathCommand NavigateTo = new NavigateToDocumentPathCommand();

        public static SaveCommand Save = new SaveCommand();

        public static SaveAsCommand SaveAs = new SaveAsCommand();

        public static SaveAllCommand SaveAll = new SaveAllCommand();

        public static CompileCommand Compile = new CompileCommand();

        public static CloseCurrentEditorCommand Close = new CloseCurrentEditorCommand();

        public static ExitCommand Exit = new ExitCommand();
    }
}