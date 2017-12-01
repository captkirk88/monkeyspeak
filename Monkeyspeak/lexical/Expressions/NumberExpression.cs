using System.Collections.Generic;
using System.IO;

namespace Monkeyspeak.Lexical.Expressions
{
    public sealed class NumberExpression : Expression<double>
    {
        public NumberExpression(SourcePosition pos, double num) : base(pos, num)
        {
        }

        public NumberExpression()
        {
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(GetValue<double>());
        }

        public override void Read(BinaryReader reader)
        {
            SetValue(reader.ReadDouble());
        }

        //Optional (defaults to adding this expression to the trigger)
        public override bool Apply(Trigger? trigger)
        {
            return base.Apply(trigger);
        }

        public override object Execute(Page page, Queue<IExpression> contents, bool addToPage = false)
        {
            return GetValue<double>();
        }
    }
}