using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.lexical.Expressions
{
    public sealed class VariableTableExpression : VariableExpression
    {
        public VariableTableExpression(ref SourcePosition pos, string varRef, string indexer) : base(ref pos, varRef)
        {
            Indexer = indexer;
        }

        public string Indexer { get; private set; }
        public bool HasIndex { get => string.IsNullOrWhiteSpace(Indexer); }
    }
}