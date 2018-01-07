using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Monkeyspeak.Editor.Commands
{
    public sealed class CompileCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            var script = Editors.Instance.Selected?.textEditor.Text;
            MonkeyspeakRunner.LoadString(script);
            MonkeyspeakRunner.Compile(Editors.Instance.Selected?.CurrentFilePath);
        }

        public override object ToolTip => "Compiles the current document";
    }
}