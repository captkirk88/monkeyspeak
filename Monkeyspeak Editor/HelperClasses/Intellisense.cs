﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using Monkeyspeak.Lexical.Expressions;

namespace Monkeyspeak.Editor.HelperClasses
{
    public static class Intellisense
    {
        private static CompletionWindow triggerCompletionWindow;
        private static CompletionWindow variableCompletionWindow;

        private static ToolTip triggerDescToolTip;

        private static List<TriggerCompletionData> triggerCompletions = new List<TriggerCompletionData>();
        private static List<VariableCompletionData> variableCompletions = new List<VariableCompletionData>();
        private static Page page;
        private static bool variableSymbolTyped = false;

        public static void InitializeTriggerListCompletion()
        {
            page = MonkeyspeakRunner.CurrentPage;
            foreach (var lib in MonkeyspeakRunner.CurrentPage.Libraries)
            {
                foreach (var trigger in lib.Handlers.Select(handler => handler.Key))
                {
                    triggerCompletions.Add(new TriggerCompletionData(page, lib, trigger));
                }
            }
        }

        public static bool CanShow => Editors.Instance.Selected != null;

        public static void GenerateTriggerListCompletion()
        {
            if (triggerCompletions.Count == 0) InitializeTriggerListCompletion();
            if (triggerCompletionWindow != null || Editors.Instance.Selected == null)
            {
                triggerCompletionWindow?.Close();
            }

            var selected = Editors.Instance.Selected;
            var textEditor = selected.textEditor;
            triggerCompletionWindow = new CompletionWindow(textEditor.TextArea)
            {
                CloseAutomatically = false,
            };
            var data = triggerCompletionWindow.CompletionList.CompletionData;
            var line = selected.CurrentLine.Trim(' ', '\t', '\n');
            foreach (var tc in triggerCompletions.Where(tc => tc.Text.IndexOf(line) >= 0 || line.CompareTo(tc.Text) <= 0))
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

        public static void GenerateVariableListCompletion()
        {
            if (variableCompletions.Count == 0) ScanForVariables();
            if (variableCompletionWindow != null || Editors.Instance.Selected == null)
            {
                variableCompletionWindow?.Close();
            }

            var selected = Editors.Instance.Selected;
            var textEditor = selected.textEditor;
            variableCompletionWindow = new CompletionWindow(textEditor.TextArea)
            {
                CloseAutomatically = false,
            };
            var data = variableCompletionWindow.CompletionList.CompletionData;
            var line = selected.CurrentLine.Trim(' ', '\t', '\n');
            foreach (var vc in variableCompletions.Where(vc => vc.Text.IndexOf(line) >= 0 || line.CompareTo(vc.Text) <= 0))
            {
                data.Add(vc);
            }
            variableCompletionWindow.SizeToContent = SizeToContent.Width;
            if (data.Count > 0) variableCompletionWindow.Show();
            variableCompletionWindow.Closed += delegate
            {
                variableCompletionWindow = null;
            };
        }

        /// <summary>
        /// Add this to the text editor's TextEntered event
        /// </summary>
        /// <param name="e">The <see cref="TextCompositionEventArgs"/> instance containing the event data.</param>
        public static void TextEntered(TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0)
            {
                if (!variableSymbolTyped && triggerCompletionWindow != null)
                {
                    variableCompletionWindow?.Close();
                    triggerCompletionWindow.CompletionList.RequestInsertion(e);
                }
                else if (variableSymbolTyped && variableCompletionWindow != null)
                {
                    triggerCompletionWindow?.Close();
                    variableCompletionWindow.CompletionList.RequestInsertion(e);
                }
            }

            if (e.Text.StartsWith(MonkeyspeakRunner.Options.VariableDeclarationSymbol.ToString()))
            {
                variableSymbolTyped = true;
                GenerateVariableListCompletion();
            }

            if (e.Text == " ")
            {
                ScanForVariables();
                variableCompletionWindow?.Close();
            }
        }

        public static void MouseHover(MouseEventArgs e)
        {
            if (triggerDescToolTip == null) triggerDescToolTip = new ToolTip();

            var selected = Editors.Instance.Selected;
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
                var completionData = new TriggerCompletionData(page, textAtOffset);
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

        public static void MouseMove(MouseEventArgs e)
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

        public static void ScanForVariables()
        {
            var selected = Editors.Instance.Selected;
            var textEditor = selected.textEditor;
            if (selected == null) return;
            Parser parser = new Parser(MonkeyspeakRunner.Engine);
            var memory = new MemoryStream(Encoding.Default.GetBytes(textEditor.Text));
            Lexer lexer = new Lexer(MonkeyspeakRunner.Engine, new SStreamReader(memory));
            foreach (var expr in parser.Parse(lexer)
                .SelectMany(trigger =>
                trigger.Contents.Where(expr => Expressions.Instance[TokenType.VARIABLE | TokenType.TABLE] == expr.GetType())))
            {
                var varExpr = expr.GetValue<string>();
                if (!variableCompletions.Any(vc => vc.Text == varExpr)) variableCompletions.Add(new VariableCompletionData(page, varExpr));
            }
            variableSymbolTyped = false;
        }
    }
}