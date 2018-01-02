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
        public override void Execute(Editor.IEditor editor)
        {
            if (editor.HighlighterLanguage == "Monkeyspeak")
            {
                var selectedWord = editor.SelectedWord;
                var selectedLine = editor.SelectedLine;
                editor.SetTextColor(Colors.Red, editor.CaretLine, selectedLine.IndexOf(selectedWord), selectedLine.IndexOf(selectedWord) + selectedWord.Length);
            }
        }

        public override void Initialize()
        {
        }

        public override void Unload()
        {
        }
    }
}