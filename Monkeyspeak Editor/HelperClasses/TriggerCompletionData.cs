using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
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

        // TODO create syntax highlighted tooltips
        public object Description
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine(Text);
                if (lib != null)
                {
                    sb.AppendLine($"Library: {lib.GetType().Name}");
                    sb.AppendLine($"Handler: {lib.Handlers.FirstOrDefault(h => h.Key == trigger).Value?.Method.Name}");
                }
                return sb.ToString();
            }
        }

        public double Priority => 1;

        public bool CanComplete(string line, string textEntered)
        {
            bool yes = false;
            if (line.Contains($"({(int)trigger.Category}"))
                yes = true;
            return yes;
        }

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text.Substring(completionSegment.Offset));
        }
    }
}