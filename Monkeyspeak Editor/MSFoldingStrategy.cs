using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using Monkeyspeak.Editor.Syntax;

namespace Monkeyspeak.Editor
{
    public class MSFoldingStrategy
    {
        public static IEnumerable<NewFolding> Generate(TextArea area)
        {
            foreach (var trigger in MonkeyspeakRunner.CurrentPage.Blocks)
            {
                yield return new NewFolding(trigger.First().SourcePosition.RawPosition, trigger.Last().SourcePosition.RawPosition);
            }
        }
    }
}