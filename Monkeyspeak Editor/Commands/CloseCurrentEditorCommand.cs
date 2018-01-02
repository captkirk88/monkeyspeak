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
    public sealed class CloseCurrentEditorCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            await Editors.Instance.Selected?.CloseAsync();
            if (Editors.Instance.IsEmpty)
            {
                var editor = Editors.Instance.Add();
                await Application.Current.Dispatcher.InvokeAsync(() => ((MahApps.Metro.Controls.MetroAnimatedTabControl)editor.Parent).SelectedItem = editor);
            }
        }
    }
}