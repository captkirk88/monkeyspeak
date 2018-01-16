using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Monkeyspeak.Editor.Commands
{
    public sealed class SaveCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            Editors.Instance.Selected?.Save();
        }

        public override object ToolTip => "Saves the current document";
    }
}