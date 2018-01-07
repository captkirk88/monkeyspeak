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
    public sealed class CloseCurrentEditorCommand : BaseCommand
    {
        public override async void Execute(object parameter)
        {
            await Editors.Instance.Selected?.CloseAsync();
            if (Editors.Instance.IsEmpty)
            {
                Application.Current.Shutdown();
            }
        }

        public override object ToolTip => "Closes the current document";
    }
}