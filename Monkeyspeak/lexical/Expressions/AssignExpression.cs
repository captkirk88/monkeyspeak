using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Monkeyspeak.Lexical;
using Monkeyspeak.Lexical.Expressions;

namespace Monkeyspeak.lexical.Expressions
{
    public class AssignExpression<T, U> : MSExpression<object>
    {
        private Expression child;

        public AssignExpression(SourcePosition pos, MSExpression<T> member, MSExpression<U> val) :
            base(pos, member.Value)
        {
            child = Assign(member, val);
        }
    }
}