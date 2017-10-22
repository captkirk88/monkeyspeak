namespace Monkeyspeak.lexical.Expressions
{
    public sealed class NumberExpression : Expression<double>
    {
        public NumberExpression(ref SourcePosition pos, double num) : base(ref pos)
        {
            Value = num;
        }
    }
}