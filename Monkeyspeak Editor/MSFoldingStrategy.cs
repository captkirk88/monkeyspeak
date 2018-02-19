using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Document;
using Monkeyspeak.Editor.Syntax;
using Monkeyspeak.Logging;

namespace Monkeyspeak.Editor
{
    public class MSFoldingStrategy
    {
        /// <summary>
        /// Generates the folding regions.
        /// </summary>
        /// <param name="area">The text area.</param>
        /// <returns></returns>
        public static IEnumerable<NewFolding> Generate(TextArea area)
        {
            MonkeyspeakRunner.LoadString(area.Document.Text);
            foreach (var block in MonkeyspeakRunner.CurrentPage.Blocks)
            {
                var first = block.First;
                var last = block.Last;
                if (first != Trigger.Undefined && last != Trigger.Undefined)
                {
                    var firstOffset = area.Document.GetLineByNumber(first.SourcePosition.Line).Offset;
                    var lastOffset = area.Document.GetLineByNumber(last.SourcePosition.Line).NextLine.Offset;
                    yield return new NewFolding(firstOffset, lastOffset)
                    {
                        IsDefinition = false
                    };
                }
            }
        }
    }
}