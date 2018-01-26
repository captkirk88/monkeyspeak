using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monkeyspeak.Editor.HelperClasses;
using Monkeyspeak.Editor.Syntax;

namespace Monkeyspeak.Editor.Commands
{
    public sealed class CompletionCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            Intellisense.GenerateTriggerListCompletion(Editors.Instance.Selected);
        }
    }
}