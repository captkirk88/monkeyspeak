﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Monkeyspeak.Editor.Controls;
using Monkeyspeak.Editor.Utils;
using Monkeyspeak.Lexical;
using Monkeyspeak.Logging;

namespace Monkeyspeak.Editor.Syntax
{
    public class SyntaxError
    {
        public EditorControl Editor { get; set; }
        public Exception Exception { get; set; }
        public SourcePosition SourcePosition { get; set; }
        public Syntax.SyntaxChecker.Severity Severity { get; set; }
    }

    public class SyntaxChecker
    {
        private static Dictionary<EditorControl, ITextMarkerService> textMarkers = new Dictionary<EditorControl, ITextMarkerService>();
        private static Page page;

        public static event Action<EditorControl> Cleared;

        public static event Action<EditorControl, MonkeyspeakException, SourcePosition, Severity> Error, Warning, Info;

        public static bool Enabled => Properties.Settings.Default.SyntaxCheckingEnabled;

        public static void Install(EditorControl editor)
        {
            var textEditor = editor.textEditor;
            editor.Unloaded += Editor_Unloaded;
            var textMarkerService = new TextMarkerService(textEditor.Document);
            textEditor.TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
            textEditor.TextArea.TextView.LineTransformers.Add(textMarkerService);
            IServiceContainer services = (IServiceContainer)textEditor.Document.ServiceProvider.GetService(typeof(IServiceContainer));
            if (services != null)
                services.AddService(typeof(ITextMarkerService), textMarkerService);
            else Logger.Debug<SyntaxChecker>("Failed to register service");
            textMarkers.Add(editor, textMarkerService);

            page = MonkeyspeakRunner.CurrentPage;
        }

        private static void Editor_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((EditorControl)sender).Unloaded -= Editor_Unloaded;
            ClearAllMarkers((EditorControl)sender);
            textMarkers.Remove((EditorControl)sender);
        }

        public static void Check(EditorControl editor, int line = -1, string text = null)
        {
            if (!Enabled) return;
            editor.Dispatcher.Invoke(() =>
            {
                SourcePosition pos = new SourcePosition(line, 1, line + 1);
                if (line == -1)
                {
                    ClearAllMarkers(editor);
                    text = editor.textEditor.Text;
                }
                else
                {
                    ClearMarker(pos, editor);
                    text = editor.textEditor.Document.GetText(editor.textEditor.Document.GetLineByNumber(line));
                }
                if (string.IsNullOrWhiteSpace(text)) return;
                using (var memory = new MemoryStream(Encoding.Default.GetBytes(text)))
                {
                    Parser parser = new Parser(MonkeyspeakRunner.Engine);
                    Lexer lexer = new Lexer(MonkeyspeakRunner.Engine, new SStreamReader(memory));
                    lexer.Error += ex => AddMarker(line == -1 ? ex.SourcePosition : pos, editor, ex.Message);
                    lexer.Error += ex => Error?.Invoke(editor, ex, line == -1 ? ex.SourcePosition : pos, Severity.Error);
                    foreach (var trigger in parser.Parse(lexer))
                    {
                        if (line != -1)
                            pos = new SourcePosition(line, trigger.SourcePosition.Column, trigger.SourcePosition.RawPosition);
                        if (page != null && !page.Libraries.Any(lib => lib.Contains(trigger.Category, trigger.Id)))
                        {
                            AddMarker(line == -1 ? trigger.SourcePosition : pos, editor, severity: Severity.Warning);
                            Warning?.Invoke(editor, new MonkeyspeakException($"{trigger} does not have a handler associated to it that could be found."), line == -1 ? trigger.SourcePosition : pos, Severity.Warning);
                        }
                    }
                }
                editor.textEditor.Focus();
            });
        }

        private static void AddMarker(Token token, EditorControl editor, string message = null, Severity severity = Severity.Error)
        {
            if (!Enabled) return;
            if (token == Token.None) return;
            var line = editor.textEditor.Document.GetLineByNumber(token.Position.Line);
            var startOffset = line.Offset + token.Position.Column;
            var textMarker = textMarkers[editor];
            if (textMarker.TextMarkers.Any(m => m.StartOffset == startOffset)) return;
            ITextMarker marker = textMarker.Create(startOffset, token.Length);
            switch (severity)
            {
                case Severity.Error:
                    marker.MarkerColor = Colors.Red;
                    marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                    break;

                case Severity.Warning:
                    marker.MarkerColor = Colors.OrangeRed;
                    marker.MarkerTypes = TextMarkerTypes.DottedUnderline;
                    break;

                case Severity.Info:
                    marker.MarkerColor = Colors.Blue;
                    marker.MarkerTypes = TextMarkerTypes.DottedUnderline;
                    break;

                default:
                    marker.MarkerColor = Colors.Red;
                    marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                    break;
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                marker.ToolTip = message;
            }
        }

        private static void AddMarker(SourcePosition pos, EditorControl editor, string message = null, Severity severity = Severity.Error)
        {
            if (!Enabled) return;
            var line = editor.textEditor.Document.GetLineByNumber(pos.Line);
            var textMarker = textMarkers[editor];
            if (textMarker.GetMarkersAtOffset(line.Offset).Count() > 0) return;
            ITextMarker marker = textMarker.Create(line.Offset, line.Length);
            switch (severity)
            {
                case Severity.Error:
                    marker.MarkerColor = Colors.Red;
                    marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                    break;

                case Severity.Warning:
                    marker.MarkerColor = Colors.Orange;
                    marker.MarkerTypes = TextMarkerTypes.DottedUnderline;
                    break;

                case Severity.Info:
                    marker.MarkerColor = Colors.Blue;
                    marker.MarkerTypes = TextMarkerTypes.DottedUnderline;
                    break;

                default:
                    marker.MarkerColor = Colors.Red;
                    marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                    break;
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                marker.ToolTip = message;
            }
        }

        public static void ClearMarker(SourcePosition sourcePosition, EditorControl editor)
        {
            var line = editor.textEditor.Document.GetLineByNumber(sourcePosition.Line);
            var textMarker = textMarkers[editor];
            textMarker.RemoveAll(marker => marker.StartOffset >= line.Offset && marker.EndOffset <= line.EndOffset);
        }

        public static void ClearAllMarkers(EditorControl editor)
        {
            var textMarker = textMarkers[editor];
            textMarker.RemoveAll(marker => true);
            Cleared?.Invoke(editor);
        }

        public enum Severity
        {
            Error, Warning, Info
        }
    }
}