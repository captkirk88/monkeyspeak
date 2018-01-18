using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Monkeyspeak.Editor.Controls;
using Monkeyspeak.Lexical;
using Monkeyspeak.Logging;

namespace Monkeyspeak.Editor.Syntax
{
    public class SyntaxChecker
    {
        private static Dictionary<EditorControl, ITextMarkerService> textMarkers = new Dictionary<EditorControl, ITextMarkerService>();

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
        }

        private static void Editor_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((EditorControl)sender).Unloaded -= Editor_Unloaded;
            textMarkers.Remove((EditorControl)sender);
        }

        public static void Check(EditorControl editor)
        {
            var text = editor.textEditor.Text;
            if (string.IsNullOrWhiteSpace(text)) return;
            using (var memory = new MemoryStream(Encoding.Default.GetBytes(text)))
            {
                Parser parser = new Parser(MonkeyspeakRunner.Engine);
                Lexer lexer = new Lexer(MonkeyspeakRunner.Engine, new SStreamReader(memory));
                try
                {
                    foreach (var token in lexer.Read())
                    {
                        if (token.Type == TokenType.TRIGGER)
                        {
                            if (Trigger.TryParse(MonkeyspeakRunner.Engine, token.GetValue(lexer), out var trigger))
                                if (MonkeyspeakRunner.CurrentPage.Libraries.All(lib => !lib.Contains(trigger.Category, trigger.Id)))
                                {
                                    // that trigger was not found in any library
                                    AddMarker(token, editor, "Trigger was not found in any libraries currently loaded.");
                                }
                        }
                    }
                }
                catch (MonkeyspeakException ex)
                {
                    ex.Log<SyntaxChecker>();
                    var pos = ex.SourcePosition;
                    AddMarker(ex.SourcePosition, editor, ex.Message);
                }
            }
        }

        private static void AddMarker(Token token, EditorControl editor, string message = null, Severity severity = Severity.Error)
        {
            var line = editor.textEditor.Document.GetLineByNumber(token.Position.Line);
            var textMarker = textMarkers[editor];
            Color color;
            switch (severity)
            {
                case Severity.Error:
                    color = Colors.Red;
                    break;

                case Severity.Warning:
                    color = Colors.Yellow;
                    break;

                case Severity.Info:
                    color = Colors.Blue;
                    break;

                default:
                    color = Colors.Red;
                    break;
            }
            ITextMarker marker = textMarker.Create(line.Offset + token.Position.Column, line.Offset + token.Position.Column + token.Length);
            marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
            marker.MarkerColor = color;
            if (!string.IsNullOrWhiteSpace(message))
            {
                marker.ToolTip = message;
            }
        }

        private static void AddMarker(SourcePosition pos, EditorControl editor, string message = null, Severity severity = Severity.Error)
        {
            var line = editor.textEditor.Document.GetLineByNumber(pos.Line);
            var textMarker = textMarkers[editor];
            Color color;
            switch (severity)
            {
                case Severity.Error:
                    color = Colors.Red;
                    break;

                case Severity.Warning:
                    color = Colors.Yellow;
                    break;

                case Severity.Info:
                    color = Colors.Blue;
                    break;

                default:
                    color = Colors.Red;
                    break;
            }
            ITextMarker marker = textMarker.Create(line.Offset + pos.Column, line.EndOffset - pos.Column);
            marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
            marker.MarkerColor = color;
            if (!string.IsNullOrWhiteSpace(message))
            {
                marker.ToolTip = message;
            }
        }

        public static void ClearMarker(SourcePosition sourcePosition, EditorControl editor)
        {
            var line = editor.textEditor.Document.GetLineByNumber(sourcePosition.Line);
            var textMarker = textMarkers[editor];
            textMarker.RemoveAll(marker => marker.StartOffset == line.Offset);
        }

        public static void ClearAllMarkers(EditorControl editor)
        {
            var textMarker = textMarkers[editor];
            textMarker.RemoveAll(marker => true);
        }

        public enum Severity
        {
            Error, Warning, Info
        }
    }
}