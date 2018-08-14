using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using Monkeyspeak.Libraries;

namespace Monkeyspeak.Editor.Syntax
{
    internal class VariableCompletionData : ICompletionData
    {
        private readonly Trigger trigger = Trigger.Undefined;
        private readonly Page page;
        private TextView text, syntaxViewer;
        private DocumentHighlighter textHighlighter, syntaxViewerHighlighter;
        private IHighlightingDefinition highlightingDef;

        private IEditor editorFoundIn;
        private object value;

        public VariableCompletionData(Page page, IEditor editorFoundIn, string varRef, object value = null)
        {
            this.page = page;
            this.editorFoundIn = editorFoundIn;
            Text = varRef ?? string.Empty;
            this.value = value;
            highlightingDef = HighlightingManager.Instance.GetDefinition("Monkeyspeak");
            this.text = new TextView();
            syntaxViewer = new TextView();
        }

        public string Text { get; private set; }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        public object Content
        {
            get
            {
                text.Document = new TextDocument(Text);
                HighlightingColorizer colorizer = new HighlightingColorizer(highlightingDef);
                text.LineTransformers.Add(colorizer);
                text.EnsureVisualLines();
                text.IsHitTestVisible = true;
                return text;
            }
        }

        public object Description
        {
            get
            {
                syntaxViewer.Document = new TextDocument(value != null ? $"{Text} = {value} {(editorFoundIn != null ? "(" + editorFoundIn.Title + ")" : "")}" : Text);
                HighlightingColorizer colorizer = new HighlightingColorizer(highlightingDef);
                syntaxViewer.LineTransformers.Add(colorizer);
                syntaxViewer.EnsureVisualLines();
                syntaxViewer.IsHitTestVisible = false;
                return syntaxViewer;
            }
        }

        public double Priority => 0;

        public IVariable Variable => page.GetVariable(Text);
        public Trigger Trigger => trigger;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            var line = textArea.Document.GetLineByOffset(completionSegment.Offset);
            textArea.Document.Replace(line.Offset, line.Length, "");
            textArea.Document.Insert(line.Offset, Text);
        }
    }
}