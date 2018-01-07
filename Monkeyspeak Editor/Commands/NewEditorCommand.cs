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
    public sealed class NewEditorCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            var editor = Editors.Instance.Add();
            Application.Current.Dispatcher.Invoke(() => ((MahApps.Metro.Controls.MetroAnimatedSingleRowTabControl)editor.Parent).SelectedItem = editor);
        }

        public override object ToolTip => "Creates a new document";
    }
}