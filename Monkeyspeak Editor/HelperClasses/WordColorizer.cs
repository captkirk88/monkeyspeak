using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
                if (start == 0 && end == 0) return;
                ChangeLinePart(line.Offset + start, line.Offset + end, ApplyChanges);
            }
        }

        private void ApplyChanges(VisualLineElement element)
        {
            // This is where you do anything with the line
            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(color));
        }
    }

    internal class FontWeightTransformer : DocumentColorizingTransformer
    {
        private readonly FontWeight weight;
        private int lineNumber, start, end;

        public FontWeightTransformer(FontWeight weight, int lineNumber, int start, int end)
        {
            this.lineNumber = lineNumber;
            this.start = start;
            this.end = end;
            this.weight = weight;
        }

        protected override void ColorizeLine(ICSharpCode.AvalonEdit.Document.DocumentLine line)
        {
            if (!line.IsDeleted && line.LineNumber == lineNumber)
            {
                try
                {
                    ChangeLinePart(line.Offset + start, line.Offset + end, ApplyChanges);
                }
                catch { }
            }
        }

        private void ApplyChanges(VisualLineElement element)
        {
            var props = element.TextRunProperties;
            var typeFace = props.Typeface;
            props.SetTypeface(new Typeface(typeFace.FontFamily, typeFace.Style, weight, typeFace.Stretch));
        }
    }
}