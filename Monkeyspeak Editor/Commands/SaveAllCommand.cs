using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Monkeyspeak.Editor.HelperClasses;

namespace Monkeyspeak.Editor.Commands
{
    public sealed class SaveAllCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            foreach (var editor in Editors.Instance.All)
            {
                if (Settings.AutoCompileScriptsOnSave)
                    MonkeyspeakCommands.Compile.Execute(editor);
                if (editor.HasChanges) editor.Save();
            }
        }

        public override object ToolTip => "Saves all open documents and compiles them if the option is enabled";
    }

    public sealed class ForceSaveAllCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            foreach (var editor in Editors.Instance.All)
                if (editor.HasChanges) editor.Save(true);
        }

        public override object ToolTip => "Saves all open documents";
    }
}