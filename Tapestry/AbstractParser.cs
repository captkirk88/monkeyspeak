using System.Collections.Generic;
using Tapestry.Expressions;

namespace Tapestry
{
    public abstract class AbstractParser
    {
        public abstract IEnumerable<Expression> Parse(AbstractLexer lexer);
    }
}