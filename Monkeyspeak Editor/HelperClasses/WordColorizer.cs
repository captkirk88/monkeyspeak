using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monkeyspeak.Editor.HelperClasses
{
    internal class WordColorizer : DocumentColorizingTransformer
    {
        private readonly Color color;
        private int lineNumber, start, end;

        public WordColorizer(Color color, int lineNumber, int start, int end)
        {
            this.lineNumber = lineNumber;
            this.start = start;
            this.end = end;
            this.color = color;
        }

        protected override void ColorizeLine(ICSharpCode.AvalonEdit.Document.DocumentLine line)
        {
            if (!line.IsDeleted && line.LineNumber == lineNumber)
            {
                ChangeLinePart(start, end, ApplyChanges);
            }
        }

        private void ApplyChanges(VisualLineElement element)
        {
            // This is where you do anything with the line
            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(color));
        }
    }
}