using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Monkeyspeak.Lexical.Expressions
{
    /// <summary>
    /// Expression pointing to a Variable reference
    /// <para>
    /// This expression does not have the value of the variable because the variable would not have
    /// been assigned yet
    /// </para>
    /// </summary>
    public class ObjectVariableExpression : Expression<string>
    {
        public ObjectVariableExpression()
        {
        }

        public ObjectVariableExpression(SourcePosition pos, string varRef) :
            base(pos, varRef.LeftOf('.'))
        {
            DesiredProperty = varRef.RightOf('.');
        }

        public string DesiredProperty { get; private set; }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(GetValue<string>());
            writer.Write(DesiredProperty);
        }

        public override void Read(BinaryReader reader)
        {
            SetValue(reader.ReadString());
            DesiredProperty = reader.ReadString();
        }

        public override object Execute(Page page, Queue<IExpression> contents, bool addToPage = false)
        {
            IVariable var = null;
            var varRef = GetValue<string>();
            if (!page.HasVariable(varRef, out var))
                if (addToPage)
                    var = page.SetVariable(new ObjectVariable(varRef));

            if (var is ObjectVariable obj)
            {
                obj.DesiredProperty = DesiredProperty;
                return obj;
            }
            return var ?? ObjectVariable.Null;
        }

        public override string ToString()
        {
            return $"{GetValue<string>()}.{DesiredProperty}";
        }
    }
}