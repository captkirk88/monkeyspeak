using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Monkeyspeak.Editor.HelperClasses
{
    internal class HighlightSelectedColorizer : DocumentColorizingTransformer
    {
        private readonly string search;

        public HighlightSelectedColorizer(string search)
        {
            this.search = search;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            int lineStartOffset = line.Offset;
            string text = CurrentContext.Document.GetText(line);

            int start = 0;
            int index;
            while ((index = text.IndexOf(search, start)) >= 0)
            {
                base.ChangeLinePart(
                    lineStartOffset + index, // startOffset
                    lineStartOffset + index + search.Length, // endOffset
                    (VisualLineElement element) =>
                    {
                        // This lambda gets called once for every VisualLineElement
                        // between the specified offsets.
                        Typeface tf = element.TextRunProperties.Typeface;
                        // Replace the typeface with a modified version of
                        // the same typeface
                        element.TextRunProperties.SetTypeface(new Typeface(
                                tf.FontFamily,
                                FontStyles.Italic,
                                FontWeights.Bold,
                                tf.Stretch
                            ));
                    });
                start = index + 1; // search for next occurrence
            }
        }
    }
}