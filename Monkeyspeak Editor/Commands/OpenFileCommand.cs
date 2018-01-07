using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Monkeyspeak.Editor.Commands
{
    public sealed class OpenFileCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            var editor = Editors.Instance.Add();
            var filePath = parameter as string;
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                editor.CurrentFilePath = filePath;
                editor.Reload();
            }
            else
            {
                editor.Open();
            }
            //Application.Current.Dispatcher.Invoke(() => ((MahApps.Metro.Controls.MetroAnimatedSingleRowTabControl)editor.Parent).SelectedItem = editor);
            editor.HasChanges = false;
        }

        public override object ToolTip => "Opens a file";
    }
}