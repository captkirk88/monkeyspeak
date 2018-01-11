using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using Monkeyspeak.Editor.Interfaces;
using Monkeyspeak.Editor.Notifications;
using Monkeyspeak.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.HelperClasses
{
    internal class TriggerCompletionData : ICompletionData
    {
        private readonly BaseLibrary lib;
        private readonly Trigger trigger;
        private readonly Page page;

        public TriggerCompletionData(Page page, BaseLibrary lib, Trigger trigger)
        {
            this.page = page;
            this.trigger = trigger;
            Text = page.GetTriggerDescription(trigger, true).Trim('\r', '\n');
            this.lib = lib;
        }

        public string Text { get; private set; }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content
        {
            get { return Text; }
        }

        public object Description
        {
            get
            {
                TextEditor syntaxViewer = new TextEditor
                {
                    IsEnabled = false,
                    SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Monkeyspeak"),
                    ShowLineNumbers = false
                };
                var sb = new StringBuilder();
                sb.AppendLine(Text);
                if (lib != null)
                {
                    var handler = lib.Handlers.FirstOrDefault(h => h.Key == trigger).Value;
                    if (handler != null)
                    {
                        var desc = handler.Method.GetCustomAttributes(typeof(TriggerDescriptionAttribute), true).FirstOrDefault() as TriggerDescriptionAttribute;
                        sb.AppendLine(desc?.Description ?? "");
                    }
                    sb.AppendLine($"Library: {lib.GetType().Name}");
                }
                syntaxViewer.Text = Text;
                return syntaxViewer;
            }
        }

        public double Priority => 1;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            var line = textArea.Document.GetLineByOffset(completionSegment.Offset);
            textArea.Document.Replace(line.Offset, line.Length, Text);
        }
    }
}