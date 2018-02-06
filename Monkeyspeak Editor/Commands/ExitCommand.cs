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
            var window = (MainWindow)Application.Current.MainWindow;

            var editors = Editors.Instance.All.ToArray();
            List<string> session = new List<string>();
            for (int i = editors.Length - 1; i >= 0; i--)
            {
                if (editors[i].HasFile)
                    session.Add(editors[i].CurrentFilePath);
                if (!await editors[i].Close())
                {
                    return;
                }
            }
            Settings.LastSession = string.Join(";", session);
            Settings.Save();
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        public override object ToolTip => "Prompts for save changes, then exits the program";
    }
}