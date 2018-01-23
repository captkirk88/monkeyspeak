using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Monkeyspeak.Editor.Controls;
using Monkeyspeak.Editor.HelperClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Monkeyspeak.Editor.Commands
{
    public sealed class ExitCommand : BaseCommand
    {
        public override async void Execute(object parameter)
        {
            var window = (MetroWindow)Application.Current.MainWindow;
            var dialog = await DialogManager.GetCurrentDialogAsync<BaseMetroDialog>(window);
            if (dialog != null)
                await DialogManager.HideMetroDialogAsync(window, dialog);

            var editors = Editors.Instance.All.ToArray();
            var settings = Properties.Settings.Default;
            List<string> session = new List<string>();
            for (int i = editors.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(editors[i].CurrentFilePath) &&
                    File.Exists(editors[i].CurrentFilePath))
                    session.Add(editors[i].CurrentFilePath);
                if (!await editors[i].CloseAsync())
                {
                    return;
                }
            }
            if (settings.RememberWindowPosition)
            {
                settings["WindowState"] = window.WindowState;
                if (settings.WindowState != WindowState.Maximized)
                {
                    settings["WindowPosition"] = new System.Drawing.Point((int)window.Left, (int)window.Top);
                    settings["WindowSizeWidth"] = window.Width;
                    settings["WindowSizeHeight"] = window.Height;
                }
            }
            settings.LastSession = string.Join(",", session);
            settings.Save();
            Application.Current.Shutdown();
        }

        public override object ToolTip => "Prompts for save changes, then exits the program";
    }
}