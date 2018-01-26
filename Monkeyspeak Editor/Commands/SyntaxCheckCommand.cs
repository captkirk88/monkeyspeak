using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monkeyspeak.Editor.Syntax;

namespace Monkeyspeak.Editor.Commands
{
    public sealed class SyntaxCheckCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            var selected = Editors.Instance.Selected;
            if (selected == null) return;
            SyntaxChecker.Check(selected);
        }

        public override object ToolTip => "Evaluates script and checks for syntax errors";
    }
}