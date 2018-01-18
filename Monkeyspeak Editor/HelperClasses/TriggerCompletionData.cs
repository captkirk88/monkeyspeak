using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using Monkeyspeak.Editor.Interfaces;
using Monkeyspeak.Editor.Notifications;
using Monkeyspeak.Libraries;
using Monkeyspeak.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Monkeyspeak.Extensions;

namespace Monkeyspeak.Editor.HelperClasses
{
    internal class TriggerCompletionData : ICompletionData
    {
        private readonly BaseLibrary lib;
        private readonly Trigger trigger = Trigger.Undefined;
        private readonly Page page;
        private TextView text, syntaxViewer;
        private DocumentHighlighter textHighlighter, syntaxViewerHighlighter;
        private IHighlightingDefinition highlightingDef;

        public TriggerCompletionData(Page page, BaseLibrary lib, Trigger trigger)
        {
            this.page = page;
            this.trigger = trigger;
            Text = page.GetTriggerDescription(trigger, true).Trim('\r', '\n');
            this.lib = lib;
            highlightingDef = HighlightingManager.Instance.GetDefinition("Monkeyspeak");
            this.text = new TextView();
            syntaxViewer = new TextView();
        }

        public TriggerCompletionData(Page page, string line)
        {
            this.page = page;
            line = line.Trim(' ');
            this.trigger = Trigger.Parse(MonkeyspeakRunner.Engine, line);
            Text = page.GetTriggerDescription(trigger, true);
            this.lib = page.Libraries.FirstOrDefault(lib => lib.Contains(trigger.Category, trigger.Id));
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
                return Text;
            }
        }

        public object Description
        {
            get
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Text)) sb.AppendLine(Text);
                if (lib != null)
                {
                    TriggerHandler handler = null;
                    if (trigger != Trigger.Undefined)
                        handler = lib.Handlers.FirstOrDefault(h => h.Key == trigger).Value;
                    if (handler != null)
                    {
                        var triggerDescriptions = ReflectionHelper.GetAllAttributesFromMethod<TriggerDescriptionAttribute>(handler.Method).ToArray();
                        sb.AppendLine(triggerDescriptions.FirstOrDefault()?.Description ?? string.Empty);
                        int arg = 0;
                        foreach (var desc in triggerDescriptions.Skip(1))
                        {
                            if (desc != null)
                                sb.AppendLine($"Param {arg++}: {desc.Description}");
                        }
                    }
                    else sb.AppendLine("No description found."); // should never happen
                    sb.AppendLine($"Library: {lib.GetType().Name}");
                }
                else
                    sb.AppendLine("This trigger has no handler loaded into the editor.");
                syntaxViewer.Document = new TextDocument(sb.ToString());
                HighlightingColorizer colorizer = new HighlightingColorizer(highlightingDef);
                syntaxViewer.LineTransformers.Add(colorizer);
                syntaxViewer.EnsureVisualLines();
                return syntaxViewer;
            }
        }

        public double Priority => 0;

        public Trigger Trigger => trigger;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            var line = textArea.Document.GetLineByOffset(completionSegment.Offset);
            textArea.Document.Replace(line.Offset, line.Length, Text);
        }
    }
}