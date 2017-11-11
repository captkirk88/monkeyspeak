using System;

namespace Monkeyspeak.Lexical.Expressions
{
    public class Expression : IExpression, IComparable<Expression>, IEquatable<Expression>
    {
        private readonly SourcePosition sourcePosition;

        protected Expression(ref SourcePosition pos)
        {
            sourcePosition = pos;
        }

        public SourcePosition Position
        {
            get { return sourcePosition; }
        }

        public object Value { get; set; }

        public int CompareTo(Expression other)
        {
            return Value.GetHashCode().CompareTo(other.GetHashCode());
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            if (obj == null) return Value == null;
            return obj is Expression && Value.Equals(((Expression)obj).Value);
        }

        public bool Equals(Expression other)
        {
            return Value.Equals(other?.Value);
        }

        public override string ToString()
        {
            return $"{Value} {sourcePosition}";
        }

        #endregion Object Overrides
    }

    public class Expression<T> : Expression, IComparable<Expression<T>>, IEquatable<Expression>, IEquatable<Expression<T>>
    {
        public Expression(ref SourcePosition pos, T val) : base(ref pos)
        {
            Value = val;
        }

        public new T Value { get; protected set; }

        public int CompareTo(Expression<T> other)
        {
            return Value.GetHashCode().CompareTo(other.GetHashCode());
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            return obj is Expression && Value.Equals(((Expression)obj).Value);
        }

        public bool Equals(Expression<T> other)
        {
            if (other == null) return Value == null;
            return Value.Equals(other.Value);
        }

        public override string ToString()
        {
            return $"{Value} {Position}";
        }

        #endregion Object Overrides
    }
}