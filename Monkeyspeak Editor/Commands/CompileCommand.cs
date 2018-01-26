using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Monkeyspeak.Editor.Controls;

namespace Monkeyspeak.Editor.Commands
{
    public sealed class CompileCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            string script = null;
            if (parameter != null && parameter is EditorControl editor)
                script = editor.textEditor.Text;
            else script = Editors.Instance.Selected?.textEditor?.Text;
            if (string.IsNullOrWhiteSpace(script)) return;
            MonkeyspeakRunner.LoadString(script);
            MonkeyspeakRunner.Compile(Editors.Instance.Selected?.CurrentFilePath);
        }

        public override object ToolTip => "Compiles the current document";
    }
}