using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Monkeyspeak.Lexical.Expressions
{
    public sealed class StringExpression : Expression<string>
    {
        public StringExpression(ref SourcePosition pos, string value)
            : base(ref pos, value) { }
    }
}