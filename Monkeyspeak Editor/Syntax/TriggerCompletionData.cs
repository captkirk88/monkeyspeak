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
using Monkeyspeak.Editor.Extensions;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Syntax
{
    public sealed class TriggerCompletionData : ICompletionData, IComparable<TriggerCompletionData>
    {
        private readonly BaseLibrary lib;
        private readonly Trigger trigger = Trigger.Undefined;
        private readonly Page page;
        private TextView descriptionViewer = null;
        private DocumentHighlighter textHighlighter, syntaxViewerHighlighter;
        private IHighlightingDefinition highlightingDef;

        public TriggerCompletionData(Page page, BaseLibrary lib, Trigger trigger)
        {
            this.page = page;
            this.trigger = trigger;
            if (trigger != Trigger.Undefined)
            {
                switch (trigger.Category)
                {
                    case TriggerCategory.Cause:
                        Indentation = 0;
                        break;

                    case TriggerCategory.Condition:
                        Indentation = 1;
                        break;

                    case TriggerCategory.Effect:
                        Indentation = 2;
                        break;

                    case TriggerCategory.Flow:
                        Indentation = 1;
                        break;

                    default:
                        break;
                }
                Text = page.GetTriggerDescription(trigger, true).Trim('\t', '\r', '\n');
                this.lib = lib;
            }
            highlightingDef = HighlightingManager.Instance.GetDefinition("Monkeyspeak");
        }

        public TriggerCompletionData(Page page, string line)
        {
            this.page = page;
            line = line.Trim(' ', '\t', '\n');
            this.trigger = Trigger.Parse(MonkeyspeakRunner.Engine, line);
            if (trigger != Trigger.Undefined)
            {
                switch (trigger.Category)
                {
                    case TriggerCategory.Cause:
                        Indentation = 0;
                        break;

                    case TriggerCategory.Condition:
                        Indentation = 1;
                        break;

                    case TriggerCategory.Effect:
                        Indentation = 2;
                        break;

                    case TriggerCategory.Flow:
                        Indentation = 1;
                        break;

                    default:
                        break;
                }
                Text = page.GetTriggerDescription(trigger, true).Trim('\t', '\r', '\n'); ;
                this.lib = page.Libraries.FirstOrDefault(lib => lib.Contains(trigger.Category, trigger.Id));
            }
            highlightingDef = HighlightingManager.Instance.GetDefinition("Monkeyspeak");
        }

        public bool IsValid => lib != null;

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

        public object DescriptionWithoutTrigger
        {
            get
            {
                if (descriptionViewer != null) return descriptionViewer;
                var sb = new StringBuilder();
                if (lib != null)
                {
                    TriggerHandler handler = null;
                    if (trigger != Trigger.Undefined)
                        handler = lib.Handlers.FirstOrDefault(h => h.Key == trigger).Value;
                    if (handler != null)
                    {
                        var triggerDescriptions = ReflectionHelper.GetAllAttributesFromMethod<TriggerDescriptionAttribute>(handler.Method).ToArray();
                        sb.AppendLine(triggerDescriptions.FirstOrDefault()?.Description ?? string.Empty);
                    }
                    else sb.AppendLine("No description found."); // should never happen
#if DEBUG
                    sb.AppendLine($"Library: {lib.GetType().Name}");
#endif
                }
                if (sb.Length > 0)
                {
                    descriptionViewer = new TextView
                    {
                        Document = new TextDocument(sb.ToString())
                    };
                    if (highlightingDef != null)
                    {
                        HighlightingColorizer colorizer = new HighlightingColorizer(highlightingDef);
                        descriptionViewer.LineTransformers.Add(colorizer);
                        descriptionViewer.EnsureVisualLines();
                    }
                    return descriptionViewer;
                }
                else return null;
            }
        }

        public object Description
        {
            get
            {
                if (descriptionViewer != null) return descriptionViewer;
                var sb = new StringBuilder();
                if (lib != null)
                {
                    if (!string.IsNullOrWhiteSpace(Text)) sb.AppendLine(Text);
                    TriggerHandler handler = null;
                    if (trigger != Trigger.Undefined)
                        handler = lib.Handlers.FirstOrDefault(h => h.Key == trigger).Value;
                    if (handler != null)
                    {
                        var triggerDescriptions = ReflectionHelper.GetAllAttributesFromMethod<TriggerDescriptionAttribute>(handler.Method).ToArray();
                        sb.AppendLine(triggerDescriptions.FirstOrDefault()?.Description ?? string.Empty);
                        var triggerParameters = ReflectionHelper.GetAllAttributesFromMethod<TriggerParameterAttribute>(handler.Method).ToArray();
                        int arg = 1;
                        foreach (var desc in triggerParameters)
                        {
                            if (desc != null)
                                sb.AppendLine($"Parameter {arg++}: {desc.Description}");
                        }
                    }
                    else sb.AppendLine("No handler found."); // should never happen
#if DEBUG
                    sb.AppendLine($"Library: {lib.GetType().Name}");
#endif
                }
                else sb.AppendLine("No handler found.");

                if (sb.Length > 0)
                {
                    descriptionViewer = new TextView
                    {
                        Document = new TextDocument(sb.ToString())
                    };
                    if (highlightingDef != null)
                    {
                        HighlightingColorizer colorizer = new HighlightingColorizer(highlightingDef);
                        descriptionViewer.LineTransformers.Add(colorizer);
                        descriptionViewer.EnsureVisualLines();
                    }
                    return descriptionViewer;
                }
                else return null;
            }
        }

        public int Indentation { get; private set; }
        public double Priority => 0;

        public Trigger Trigger => trigger;

        public BaseLibrary Library => lib;

        public string Prepare()
        {
            string indent = string.Empty;
            for (int i = 0; i <= Indentation - 1; i++) indent += '\t';
            return indent + Text;
        }

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            var line = textArea.Document.GetLineByOffset(completionSegment.Offset);
            textArea.Document.Replace(line.Offset, line.Length, "");
            textArea.Document.Insert(line.Offset, Prepare());
        }

        public int CompareTo(TriggerCompletionData other)
        {
            if (this.Trigger.Id > other.Trigger.Id) return 1;
            if (this.Trigger.Id < other.Trigger.Id) return -1;
            return 0;
        }
    }
}