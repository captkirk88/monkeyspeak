using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Monkeyspeak.Editor.Commands
{
    public sealed class CompileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var script = Editors.Instance.Selected?.textEditor.Text;
            MonkeyspeakRunner.LoadString(script);
            MonkeyspeakRunner.Compile(Editors.Instance.Selected?.CurrentFilePath);
        }
    }
}