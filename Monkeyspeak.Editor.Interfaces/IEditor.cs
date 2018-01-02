using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monkeyspeak.Editor
{
    /// <summary>
    /// Interface to a Editor instance
    /// </summary>
    public interface IEditor
    {
        /// <summary>
        /// Gets the syntax highlighter language.
        /// </summary>
        /// <value>
        /// The syntax highlighter language.
        /// </value>
        string HighlighterLanguage { get; }

        /// <summary>
        /// Gets the caret line, requires editor to have focus or 0.
        /// </summary>
        /// <value>
        /// The caret line.
        /// </value>
        int CaretLine { get; }

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
        string SelectedWord { get; }

        /// <summary>
        /// Gets the selected line.
        /// </summary>
        /// <value>
        /// The selected line.
        /// </value>
        string SelectedLine { get; }

        /// <summary>
        /// Inserts the text at the specified line, moving any lines below it down.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="text">The text.</param>
        void InsertLine(int line, string text);

        /// <summary>
        /// Adds the text at the end of the current editor.
        /// </summary>
        /// <param name="text">The text.</param>
        void AddLine(string text);

        /// <summary>
        /// Sets the text color by navigating to the specified line and setting the color between the start and end position.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        void SetTextColor(Color color, int line, int start, int end);
    }
}