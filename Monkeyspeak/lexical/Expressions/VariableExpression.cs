namespace Monkeyspeak.Lexical.Expressions
{
    /// <summary>
    /// Expression pointing to a Variable reference
    /// <para>This expression does not have the value of the variable because the variable would not have been assigned yet</para>
    /// </summary>
    public class VariableExpression : Expression<string>
    {
        public VariableExpression(ref SourcePosition pos, string varRef) : base(ref pos, varRef)
        {
        }
    }
}