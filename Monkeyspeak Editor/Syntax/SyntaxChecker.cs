using System;
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
using Monkeyspeak.Lexical;
using Monkeyspeak.Logging;

namespace Monkeyspeak.Editor.Syntax
{
    public class SyntaxChecker
    {
        private static Dictionary<EditorControl, ITextMarkerService> textMarkers = new Dictionary<EditorControl, ITextMarkerService>();
        private static Page page;
        private static ToolTip syntaxErrorToolTip;

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
            textMarkers.Remove((EditorControl)sender);
        }

        public static void Check(EditorControl editor)
        {
            ClearAllMarkers(editor);
            var text = editor.textEditor.Text;
            if (string.IsNullOrWhiteSpace(text)) return;
            using (var memory = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                Parser parser = new Parser(MonkeyspeakRunner.Engine);
                Lexer lexer = new Lexer(MonkeyspeakRunner.Engine, new SStreamReader(memory));
                try
                {
                    foreach (var trigger in parser.Parse(lexer))
                    {
                        if (page != null && !page.Libraries.Any(lib => lib.Contains(trigger.Category, trigger.Id)))
                        {
                            AddMarker(lexer.CurrentSourcePosition, editor, $"{trigger} does not have a handler associated to it that could be found.", Severity.Warning);
                        }
                    }
                }
                catch (MonkeyspeakException ex)
                {
                    var pos = ex.SourcePosition;
                    AddMarker(ex.SourcePosition, editor, ex.Message);
                }
            }
        }

        public static bool MouseHover(EditorControl editor, MouseEventArgs e)
        {
            var pos = editor.textEditor.TextArea.TextView.GetPositionFloor(e.GetPosition(editor.textEditor.TextArea.TextView) + editor.textEditor.TextArea.TextView.ScrollOffset);
            bool inDocument = pos.HasValue;
            if (inDocument)
            {
                TextLocation logicalPosition = pos.Value.Location;
                int offset = editor.textEditor.Document.GetOffset(logicalPosition);

                var markersAtOffset = textMarkers[editor].GetMarkersAtOffset(offset);
                ITextMarker markerWithToolTip = markersAtOffset.FirstOrDefault(marker => marker.ToolTip != null);

                if (markerWithToolTip != null)
                {
                    if (syntaxErrorToolTip == null)
                    {
                        syntaxErrorToolTip = new ToolTip();
                        syntaxErrorToolTip.Closed += delegate
                        { syntaxErrorToolTip = null; };
                        syntaxErrorToolTip.PlacementTarget = editor.textEditor;
                        syntaxErrorToolTip.Content = markerWithToolTip.ToolTip;
                        syntaxErrorToolTip.IsOpen = true;
                        e.Handled = true;
                        return true;
                    }
                }
            }
            return false;
        }

        public static void MouseMove(EditorControl editor, MouseEventArgs e)
        {
            if (syntaxErrorToolTip != null) syntaxErrorToolTip.IsOpen = false;
        }

        private static void AddMarker(Token token, EditorControl editor, string message = null, Severity severity = Severity.Error)
        {
            var line = editor.textEditor.Document.GetLineByNumber(token.Position.Line);
            var textMarker = textMarkers[editor];
            ITextMarker marker = textMarker.Create(line.Offset, line.Length);
            switch (severity)
            {
                case Severity.Error:
                    marker.MarkerColor = Colors.Red;
                    marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                    break;

                case Severity.Warning:
                    marker.MarkerColor = Colors.OrangeRed;
                    marker.MarkerTypes = TextMarkerTypes.NormalUnderline;
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
            var line = editor.textEditor.Document.GetLineByNumber(pos.Line);
            var textMarker = textMarkers[editor];
            ITextMarker marker = textMarker.Create(line.Offset, line.Length);
            switch (severity)
            {
                case Severity.Error:
                    marker.MarkerColor = Colors.Red;
                    marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                    break;

                case Severity.Warning:
                    marker.MarkerColor = Colors.Orange;
                    marker.MarkerTypes = TextMarkerTypes.NormalUnderline;
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