using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Monkeyspeak.Editor
{
    [Serializable]
    public enum AppColor
    {
        Blue,
        Red,
        Green,
        Purple,
        Orange,
        Lime,
        Emerald,
        Teal,
        Cyan,
        Cobalt,
        Indigo,
        Violet,
        Pink,
        Magenta,
        Crimson,
        Amber,
        Yellow,
        Brown,
        Olive,
        Steel,
        Mauve,
        Taupe,
        Sienna
    }

    [Serializable]
    public enum AppTheme { Light, Dark }

    /// <summary>
    /// Interface to a Editor instance
    /// </summary>
    public interface IEditor
    {
        event Action<IEditor> Closing;

        event Action<string, int> LineAdded, LineRemoved;

        event Action<IEditor, string, int> Typing;

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        string Title { get; }

        /// <summary>
        /// Gets the syntax highlighter language.
        /// </summary>
        /// <value>
        /// The syntax highlighter language.
        /// </value>
        string HighlighterLanguage { get; set; }

        /// <summary>
        /// Gets the caret line, requires editor to have focus or 0.
        /// </summary>
        /// <value>
        /// The caret line.
        /// </value>
        int CaretLine { get; }

        /// <summary>
        /// Gets the caret column, requires editor to have focus or 0.
        /// </summary>
        /// <value>
        /// The caret column.
        /// </value>
        int CaretColumn { get; }

        /// <summary>
        /// Gets a value indicating whether the editor has changes.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the editor has changes; otherwise, <c>false</c>.
        /// </value>
        bool HasChanges { get; }

        /// <summary>
        /// Gets the lines.
        /// </summary>
        /// <value>
        /// The lines.
        /// </value>
        IList<string> Lines { get; }

        /// <summary>
        /// Gets the word count.
        /// </summary>
        /// <value>
        /// The word count.
        /// </value>
        int WordCount { get; }

        /// <summary>
        /// Gets the selected word.
        /// </summary>
        /// <value>
        /// The selected word.
        /// </value>
        string SelectedText { get; }

        /// <summary>
        /// Gets the selected line.
        /// </summary>
        /// <value>
        /// The selected line.
        /// </value>
        string SelectedLine { get; }

        /// <summary>
        /// Gets the current line.
        /// </summary>
        /// <value>
        /// The current line.
        /// </value>
        string CurrentLine { get; }

        /// <summary>
        /// Inserts the text at the caret's line, moving any lines below it down.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="text">The text.</param>
        void InsertAtCaretLine(string text);

        /// <summary>
        /// Adds the text at the end of the current editor.
        /// </summary>
        /// <param name="text">The text.</param>
        void AddLine(string text, bool allowUndo = true);

        /// <summary>
        /// Adds the text at the end of the current editor.
        /// </summary>
        /// <param name="text">The text.</param>
        void AddLine(string text, Color color, bool allowUndo = true);

        /// <summary>
        /// Sets the text color by navigating to the specified line and setting the color between the start and end position.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        void SetTextColor(Color color, int line, int start, int end);

        /// <summary>
        /// Sets the text <seealso cref="FontWeights"/> (thin, normal, bold, extra bold).
        /// </summary>
        /// <param name="weight">The weight.</param>
        /// <param name="line">The line.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        void SetTextWeight(FontWeight weight, int line, int start, int end);
    }
}