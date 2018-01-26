using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Commands
{
    public sealed class SaveAsCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            var selected = Editors.Instance.Selected;
            if (selected != null)
            {
                if (Properties.Settings.Default.AutoCompileScriptsOnSave)
                    MonkeyspeakCommands.Compile.Execute(selected);
                selected.SaveAs();
            }
        }

        public override object ToolTip => "Saves the current document to a different file";
    }
}