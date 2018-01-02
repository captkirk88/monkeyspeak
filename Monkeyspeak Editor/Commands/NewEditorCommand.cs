using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Monkeyspeak.Editor.Commands
{
    public sealed class NewEditorCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var editor = Editors.Instance.Add();
            Application.Current.Dispatcher.Invoke(() => ((MahApps.Metro.Controls.MetroAnimatedTabControl)editor.Parent).SelectedItem = editor);
        }
    }
}