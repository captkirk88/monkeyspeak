using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using MahApps.Metro;
using Monkeyspeak.Editor.Controls;
using Monkeyspeak.Lexical.Expressions;

namespace Monkeyspeak.Editor.HelperClasses
{
    public static class Intellisense
    {
        private static CompletionWindow triggerCompletionWindow;
        private static CompletionWindow variableCompletionWindow;

        private static ToolTip triggerDescToolTip;

        private static List<TriggerCompletionData> triggerCompletions = new List<TriggerCompletionData>();

        private static Page page;

        public static void InitializeTriggerListCompletion()
        {
            foreach (var lib in MonkeyspeakRunner.CurrentPage.Libraries)
            {
                foreach (var trigger in lib.Handlers.Select(handler => handler.Key))
                {
                    triggerCompletions.Add(new TriggerCompletionData(MonkeyspeakRunner.CurrentPage, lib, trigger));
                }
            }
        }

        public static void GenerateTriggerListCompletion(EditorControl editor)
        {
            if (editor == null) return;
            if (triggerCompletions.Count == 0) InitializeTriggerListCompletion();
            if (triggerCompletionWindow != null)
            {
                triggerCompletionWindow?.Close();
            }

            var selected = editor;
            var textEditor = selected.textEditor;
            triggerCompletionWindow = new CompletionWindow(textEditor.TextArea)
            {
                CloseAutomatically = false,
            };
            var data = triggerCompletionWindow.CompletionList.CompletionData;
            var line = selected.CurrentLine.Trim(' ', '\t', '\n');
            foreach (var tc in triggerCompletions.Where(tc => tc.Text.IndexOf(line) >= 0 || line.CompareTo(tc.Text) == 0))
            {
                data.Add(tc);
            }
            triggerCompletionWindow.SizeToContent = SizeToContent.Width;
            if (data.Count > 0) triggerCompletionWindow.Show();
            triggerCompletionWindow.Closed += delegate
            {
                triggerCompletionWindow = null;
            };
        }

        /// <summary>
        /// Add this to the text editor's TextEntered event
        /// </summary>
        /// <param name="e">The <see cref="TextCompositionEventArgs"/> instance containing the event data.</param>
        public static void TextEntered(TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && triggerCompletionWindow != null)
            {
                triggerCompletionWindow.CompletionList.RequestInsertion(e);
            }
        }

        public static void MouseHover(EditorControl editor, MouseEventArgs e)
        {
            if (triggerDescToolTip == null) triggerDescToolTip = new ToolTip();

            var selected = editor;
            var textEditor = selected.textEditor;

            var textArea = textEditor.TextArea;
            var mousePos = textEditor.GetPositionFromPoint(e.GetPosition(textEditor));
            if (mousePos != null)
            {
                var line = mousePos.Value.Line;
                var offset = textEditor.Document.GetOffset(line, 1);
                if (offset >= textEditor.Document.TextLength) offset--;
                if (offset <= 0) offset++;
                var textAtOffset = textEditor.Document.GetText(textEditor.Document.GetLineByNumber(line));
                if (string.IsNullOrWhiteSpace(textAtOffset))
                {
                    triggerDescToolTip.IsOpen = false;
                    return;
                }
                var completionData = new TriggerCompletionData(MonkeyspeakRunner.CurrentPage, textAtOffset);
                if (completionData.Trigger == Trigger.Undefined)
                {
                    triggerDescToolTip.IsOpen = false;
                    return;
                }
                triggerDescToolTip.Content = completionData.Description;
                triggerDescToolTip.PlacementTarget = selected;
                triggerDescToolTip.IsOpen = true;
                e.Handled = true;
            }
        }

        public static void MouseMove(EditorControl editor, MouseEventArgs e)
        {
            if (triggerDescToolTip != null) triggerDescToolTip.IsOpen = false;
        }

        public static void Close()
        {
            if (triggerCompletionWindow != null)
                triggerCompletionWindow.Close();
            if (variableCompletionWindow != null)
                variableCompletionWindow.Close();
            if (triggerDescToolTip != null)
                triggerDescToolTip.IsOpen = false;
        }
    }
}