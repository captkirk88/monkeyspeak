using Monkeyspeak.Editor;
using Monkeyspeak.Editor.Interfaces.Plugins;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monkeyspeak.Test.Plugin
{
    /// <summary>
    /// Colorizes the highlighted word Red
    /// </summary>
    /// <seealso cref="Monkeyspeak.Editor.Plugins.Plugin" />
    public class MyTestPlugin : Editor.Plugins.Plugin
    {
        public override void Initialize()
        {
        }

        public override void OnEditorSelectionChanged(IEditor editor)
        {
            var selectedWord = editor.SelectedText;
            var selectedLine = editor.SelectedLine;
            Logger.Debug<MyTestPlugin>($"Line: {editor.CaretLine - 1}, Start: {selectedLine.IndexOf(selectedWord)}, End: {selectedLine.IndexOf(selectedWord) + selectedWord.Length}");
            editor.SetTextColor(Colors.Red, editor.CaretLine - 1, selectedLine.IndexOf(selectedWord), selectedLine.IndexOf(selectedWord) + selectedWord.Length);
        }

        public override void OnEditorTextChanged(IEditor editor)
        {
            throw new NotImplementedException();
        }

        public override void Unload()
        {
        }
    }
}