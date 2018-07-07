using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Monkeyspeak.Editor.HelperClasses;
using Monkeyspeak.Editor.Syntax;
using Monkeyspeak.Editor.Utils;
using Monkeyspeak.Extensions;
using Monkeyspeak.Lexical.Expressions;
using Monkeyspeak.Utils;

namespace Monkeyspeak.Editor.Syntax
{
    public static class Intellisense
    {
        private static CompletionWindow triggerCompletionWindow;

        private static List<ICompletionData> triggerCompletions = new List<ICompletionData>();

        private static Page page;

        public static bool Enabled => Settings.Intellisense;
        public static List<ICompletionData> TriggerCompletions { get => triggerCompletions; }
        public static bool IsOpen { get => triggerCompletionWindow != null && triggerCompletionWindow.IsVisible; }

        public static IEnumerable<ICompletionData> GetTriggerCompletionData(EditorControl editor = null, bool forceRefresh = false)
        {
            MonkeyspeakRunner.WarmUp();
            if (forceRefresh)
            {
                TriggerCompletions.Clear();
            }
            if (TriggerCompletions.Count == 0)
            {
                foreach (var lib in MonkeyspeakRunner.CurrentPage.Libraries.OrderByDescending(l => l.GetType().Name))
                {
                    foreach (var kv in lib.Handlers.OrderBy(kv => kv.Key.Id))
                    {
                        yield return new TriggerCompletionData(MonkeyspeakRunner.CurrentPage, lib, kv.Key);
                    }
                }

                List<VariableCompletionData> vars = new List<VariableCompletionData>();
                try
                {
                    Page page = MonkeyspeakRunner.CurrentPage;
                    if (editor != null)
                    {
                        page = MonkeyspeakRunner.LoadString(editor.Text);
                        foreach (var trig in page.Triggers)
                        {
                            foreach (var expr in page.Triggers.SelectMany(t => t.Contents.OfType<VariableExpression>()))
                            {
                                expr.Execute(page, new Queue<IExpression>(trig.Contents), true);
                            }
                        }
                    }

                    foreach (var var in page.Scope)
                    {
                        vars.AddIfUnique(new VariableCompletionData(MonkeyspeakRunner.CurrentPage, var.Name, var.Value));
                    }
                }
                catch
                {
                    vars.Clear();
                    foreach (var var in MonkeyspeakRunner.CurrentPage.Scope)
                    {
                        vars.AddIfUnique(new VariableCompletionData(MonkeyspeakRunner.CurrentPage, var.Name, var.Value));
                    }
                }

                foreach (var var in vars)
                {
                    yield return var;
                }
            }
        }

        public static void GenerateTriggerListCompletion(EditorControl editor)
        {
            if (!Enabled || editor == null) return;

            foreach (var cd in GetTriggerCompletionData(editor, true))
            {
                TriggerCompletions.AddIfUnique(cd);
            }

            var selected = editor;
            var textEditor = selected.textEditor;
            if (triggerCompletionWindow == null)
            {
                triggerCompletionWindow = new CompletionWindow(textEditor.TextArea)
                {
                    CloseAutomatically = false,
                    CloseWhenCaretAtBeginning = false,
                    Background = ThemeHelper.ToThemeBackground()
                };
                Style windowStyle = new Style(typeof(CompletionWindow), Application.Current.MainWindow.Style);
                windowStyle.Setters.Add(new Setter(CompletionWindow.WindowStyleProperty, WindowStyle.None));
                windowStyle.Setters.Add(new Setter(CompletionWindow.ResizeModeProperty, ResizeMode.NoResize));
                windowStyle.Setters.Add(new Setter(CompletionWindow.BorderThicknessProperty, new Thickness(0)));
                triggerCompletionWindow.Style = windowStyle;
            }
            var data = triggerCompletionWindow.CompletionList.CompletionData;
            data.Clear();
            var line = selected.CurrentLine.Trim(' ', '\t', '\n');
            foreach (var tc in triggerCompletions
                .Where(tc => tc.Text.IndexOf(line, StringComparison.InvariantCultureIgnoreCase) >= 0 || line.CompareTo(tc.Text) == 0)
                .OrderBy(t => t is TriggerCompletionData ? ((TriggerCompletionData)t).Trigger.Category.ToString() : ((VariableCompletionData)t).Variable.Name))
            {
                data.Add(tc);
            }

            Style listBoxStyle = new Style(typeof(CompletionListBox), triggerCompletionWindow.CompletionList.ListBox.Style);
            listBoxStyle.Setters.Add(new Setter(CompletionListBox.BackgroundProperty, ThemeHelper.ToThemeBackground()));
            listBoxStyle.Setters.Add(new Setter(CompletionListBox.ForegroundProperty, ThemeHelper.ToThemeForeground()));
            triggerCompletionWindow.CompletionList.ListBox.Style = listBoxStyle;

            triggerCompletionWindow.SizeToContent = SizeToContent.Width;
            if (data.Count > 0)
            {
                triggerCompletionWindow.Show();
            }
            triggerCompletionWindow.Closed += delegate
            {
                triggerCompletionWindow = null;
                editor.textEditor.Focus();
            };
        }

        /// <summary>
        /// Add this to the text editor's TextEntered event
        /// </summary>
        /// <param name="e">
        /// The <see cref="TextCompositionEventArgs"/> instance containing the event data.
        /// </param>
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
            ToolTipManager.Opened = false;
            return false;
        }

        public static void Close()
        {
            if (triggerCompletionWindow != null)
                triggerCompletionWindow.Close();
        }
    }
}