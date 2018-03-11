using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Monkeyspeak.Editor.Controls;

namespace Monkeyspeak.Editor.Commands
{
    public sealed class OpenFileCommand : BaseCommand
    {
        public override async void Execute(object parameter)
        {
            var filePath = parameter as string;
            EditorControl editor = Editors.Instance.Add();
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                editor.CurrentFilePath = filePath;
                await editor.Reload();
            }
            else
            {
                if (!editor.Open()) await editor.Close();
            }
            //Application.Current.Dispatcher.Invoke(() => ((MahApps.Metro.Controls.MetroAnimatedSingleRowTabControl)editor.Parent).SelectedItem = editor);
            editor.HasChanges = false;
        }

        public override object ToolTip => "Opens a file";
    }
}