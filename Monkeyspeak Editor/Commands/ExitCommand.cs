using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
            var window = (MetroWindow)Application.Current.MainWindow;
            var dialog = await DialogManager.GetCurrentDialogAsync<BaseMetroDialog>(window);
            if (dialog != null)
                await DialogManager.HideMetroDialogAsync(window, dialog);

            var editors = Editors.Instance.All.ToArray();
            for (int i = editors.Length - 1; i >= 0; i--)
                await editors[i].CloseAsync();
            Application.Current.Shutdown();
        }
    }
}