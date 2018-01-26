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
            var selected = Editors.Instance.Selected;
            if (selected == null) return;
            if (Properties.Settings.Default.AutoCompileScriptsOnSave)
                MonkeyspeakCommands.Compile.Execute(selected);
            selected.Save();
        }

        public override object ToolTip => "Saves the current document and compiles if the option is enabled";
    }
}