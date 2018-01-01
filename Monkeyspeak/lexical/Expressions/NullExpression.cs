using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Lexical.Expressions
{
    public sealed class NullExpression : Expression<object>
    {
        private static NullExpression instance;

        static NullExpression()
        {
            var sourcePos = new SourcePosition();
            instance = new NullExpression(sourcePos);
        }

        public static NullExpression Instance => instance;

        public NullExpression(SourcePosition pos) : base(pos, null)
        {
        }

        public NullExpression(SourcePosition pos, object value) : base(pos, null)
        {
        }
    }
}