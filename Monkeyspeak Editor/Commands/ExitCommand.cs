using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Monkeyspeak.Editor.Commands
{
    public sealed class ExitCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var editors = Editors.Instance.All.ToArray();
            for (int i = editors.Length - 1; i >= 0; i--)
                await editors[i].CloseAsync();
            Application.Current.Shutdown();
        }
    }
}