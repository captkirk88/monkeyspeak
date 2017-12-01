using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Monkeyspeak.Lexical.Expressions
{
    /// <summary>
    /// Expression pointing to a Variable reference
    /// <para>This expression does not have the value of the variable because the variable would not have been assigned yet</para>
    /// </summary>
    public class VariableExpression : Expression<string>
    {
        public VariableExpression()
        {
        }

        public VariableExpression(SourcePosition pos, string varRef) : base(pos, varRef)
        {
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(GetValue<string>());
        }

        public override void Read(BinaryReader reader)
        {
            SetValue(reader.ReadString());
        }

        public override object Execute(Page page, Queue<IExpression> contents, bool addToPage = false)
        {
            IVariable var;
            var varRef = GetValue<string>();
            if (!page.HasVariable(varRef, out var))
                if (addToPage)
                    var = page.SetVariable(varRef, null, false);
            return var;
        }
    }
}