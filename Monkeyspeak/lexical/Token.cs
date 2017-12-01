using Monkeyspeak.Lexical;
using System.Linq;
using System.Runtime.InteropServices;

namespace Monkeyspeak
{
    public enum TokenType : byte
    {
        NONE = 0,
        // Majority of these won't be used but it is nice to have...

        COMMENT, ASSIGN,
        PLUS, MINUS, MULTIPLY, DIVIDE, MOD, POWER, // Math operators
        LPAREN, RPAREN, LBRACE, RBRACE, LBRACKET, RBRACKET, COMMA, COLON, END_STATEMENT, END_OF_FILE,
        CONCAT, // String operators
        NOT, AND, OR, // Boolean operators
        LESS_THEN, LESS_EQUAL, EQUAL, GREATER_EQUAL, GREATER_THEN, NOT_EQUAL, // Comparison operators
        TRUE, FALSE, // Constant types
        WORD, LITERAL,

        // custom
        TRIGGER, VARIABLE, TABLE, REFERENCE, STRING_LITERAL, NUMBER, PREPROCESSOR
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Token
    {
        public static readonly Token None = new Token(TokenType.NONE);

        private SourcePosition _position;
        private TokenType _type;
        private long valueStart;
        private int valueLength;

        public Token(TokenType type, long valueStart, int valueLength, SourcePosition position)
        {
            _type = type;
            this.valueStart = valueStart;
            this.valueLength = valueLength;
            _position = position;
        }

        public Token(TokenType type)
        {
            this._type = type;
            this.valueStart = 0;
            this.valueLength = 0;
            _position = new SourcePosition();
        }

        public SourcePosition Position
        {
            get { return _position; }
            internal set { _position = value; }
        }

        public TokenType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// Gets or sets the value start position within the lexer.
        /// </summary>
        /// <value>
        /// The value start position as located in the lexer.
        /// </value>
        public long ValueStartPosition { get => valueStart; set => valueStart = value; }

        /// <summary>
        /// Gets or sets the length of the value to be looked up in the lexer.
        /// </summary>
        /// <value>
        /// The length (see above).
        /// </value>
        public int Length { get => valueLength; set => valueLength = value; }

        public string GetValue(AbstractLexer lexer)
        {
            return new string(lexer.Read(valueStart, valueLength).ToArray());
        }

        public override string ToString()
        {
            return $"Type: {Type} at {Position.ToString()}";
        }

        public static bool operator ==(Token a, Token b) => a.Type == b.Type && a.valueStart == b.valueStart;

        public static bool operator !=(Token a, Token b) => a.Type != b.Type && a.valueStart != b.valueStart;
    }
}