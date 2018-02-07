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
            EditorControl editor = null;
            string script = null;
            if (parameter != null && parameter is EditorControl e)
                editor = e;
            else if (Editors.Instance.Selected != null)
                editor = Editors.Instance.Selected;

            if (editor == null || !editor.HasFile) return;
            script = editor.Text;
            if (string.IsNullOrWhiteSpace(script)) return;
            MonkeyspeakRunner.LoadString(script);
            MonkeyspeakRunner.Compile(editor.CurrentFilePath);
        }

        public override object ToolTip => "Compiles the current document";
    }
}