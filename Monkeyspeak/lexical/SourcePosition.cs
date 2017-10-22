using System.Runtime.InteropServices;

namespace Monkeyspeak.lexical
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SourcePosition
    {
        private int rawPos;

        private int line;

        private int col;

        public SourcePosition(int line, int column, int rawPos)
        {
            this.line = line;
            col = column;
            this.rawPos = rawPos;
        }

        /// <summary>
        /// Gets the line.
        /// </summary>
        /// <value>
        /// The line.
        /// </value>
        public int Line
        {
            get { return line; }
        }

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <value>
        /// The column.
        /// </value>
        public int Column
        {
            get { return col; }
        }

        /// <summary>
        /// Gets the raw character position.  Useful for looking up the location from a string.Substring call.
        /// </summary>
        /// <value>
        /// The character position.
        /// </value>
        public int RawPosition
        {
            get { return rawPos; }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SourcePosition))
            {
                return false;
            }
            SourcePosition pos = (SourcePosition)obj;
            return line == pos.line && this.col == pos.col;
        }

        public override string ToString()
        {
            return $"Line {line}, Column {col}";
        }
    }
}