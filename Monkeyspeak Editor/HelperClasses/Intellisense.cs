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
using MahApps.Metro.Controls;
using Monkeyspeak.Editor.Controls;
using Monkeyspeak.Editor.Extensions;
using Monkeyspeak.Editor.Syntax;
using Monkeyspeak.Editor.Utils;
using Monkeyspeak.Lexical.Expressions;
using Monkeyspeak.Utils;

namespace Monkeyspeak.Editor.HelperClasses
{
    public static class Intellisense
    {
        private static CompletionWindow triggerCompletionWindow;
        private static CompletionWindow variableCompletionWindow;

        private static List<TriggerCompletionData> triggerCompletions = new List<TriggerCompletionData>();

        private static Page page;

        public static bool Enabled { get; set; }

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
            if (!Enabled || editor == null) return;
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
                CloseWhenCaretAtBeginning = true
            };
            Style windowStyle = new Style(typeof(CompletionWindow), Application.Current.MainWindow.Style);
            windowStyle.Setters.Add(new Setter(CompletionWindow.WindowStyleProperty, WindowStyle.None));
            windowStyle.Setters.Add(new Setter(CompletionWindow.ResizeModeProperty, ResizeMode.NoResize));
            windowStyle.Setters.Add(new Setter(CompletionWindow.BorderThicknessProperty, new Thickness(0)));
            triggerCompletionWindow.Style = windowStyle;
            var data = triggerCompletionWindow.CompletionList.CompletionData;
            var line = selected.CurrentLine.Trim(' ', '\t', '\n');
            foreach (var tc in triggerCompletions.Where(tc => tc.Text.IndexOf(line, StringComparison.InvariantCultureIgnoreCase) >= 0 || line.CompareTo(tc.Text) == 0))
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
            if (!Enabled) return;
            if (triggerCompletionWindow != null)
            {
                triggerCompletionWindow.CompletionList.RequestInsertion(e);
            }
        }

        public static bool MouseHover(EditorControl editor, object sender, MouseEventArgs e)
        {
            if (!Enabled) return false;

            var textEditor = editor.textEditor;

            var textArea = textEditor.TextArea;
            var pos = textEditor.GetPositionFromPoint(Mouse.GetPosition(textEditor));
            bool inDocument = pos.HasValue;
            if (inDocument)
            {
                var line = pos.Value.Line;
                var lineContents = textEditor.Document.GetLineByNumber(line);
                var offset = lineContents.Offset;
                var textAtOffset = textEditor.Document.GetText(lineContents).Trim(' ', '\t', '\n');
                if (string.IsNullOrWhiteSpace(textAtOffset))
                {
                    ToolTipManager.Opened = false;
                    return false;
                }
                var completionData = new TriggerCompletionData(MonkeyspeakRunner.CurrentPage, textAtOffset);
                ToolTipManager.Add(completionData.Description);
                ToolTipManager.Target = editor;
                e.Handled = true;
                return true;
            }
            return false;
        }

        public static void Close()
        {
            if (triggerCompletionWindow != null)
                triggerCompletionWindow.Close();
            if (variableCompletionWindow != null)
                variableCompletionWindow.Close();
        }
    }
}