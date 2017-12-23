using System;
using System.Collections.Generic;
using System.IO;

namespace Monkeyspeak.Lexical.Expressions
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Monkeyspeak.Lexical.Expressions.IExpression" />
    /// <seealso cref="System.IComparable{Monkeyspeak.Lexical.Expressions.Expression}" />
    /// <seealso cref="System.IEquatable{Monkeyspeak.Lexical.Expressions.Expression}" />
    public class Expression : IExpression, IComparable<Expression>, IEquatable<Expression>
    {
        private readonly SourcePosition sourcePosition;
        private object value;

        protected Expression()
        {
            sourcePosition = new SourcePosition();
        }

        protected Expression(SourcePosition pos)
        {
            sourcePosition = pos;
        }

        public virtual T GetValue<T>()
        {
            return (T)value;
        }

        public virtual void SetValue(object value)
        {
            this.value = value;
        }

        public SourcePosition Position
        {
            get { return sourcePosition; }
        }

        public int CompareTo(Expression other)
        {
            return GetValue<object>().GetHashCode().CompareTo(other.GetHashCode());
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return obj is Expression && GetValue<object>() != null && GetValue<object>().Equals(((Expression)obj).GetValue<object>());
        }

        public bool Equals(Expression other)
        {
            return GetValue<object>().Equals(other?.GetValue<object>());
        }

        public override string ToString()
        {
            return $"{GetValue<object>()} {sourcePosition}";
        }

        public virtual void Apply(Trigger? trigger)
        {
            trigger?.contents?.Add(this);
        }

        public virtual void Write(BinaryWriter writer)
        {
        }

        public virtual void Read(BinaryReader reader)
        {
        }

        public virtual object Execute(Page page, Queue<IExpression> contents, bool addToPage = false)
        {
            throw new NotImplementedException();
        }

        #endregion Object Overrides
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Monkeyspeak.Lexical.Expressions.IExpression" />
    /// <seealso cref="System.IComparable{Monkeyspeak.Lexical.Expressions.Expression}" />
    /// <seealso cref="System.IEquatable{Monkeyspeak.Lexical.Expressions.Expression}" />
    public class Expression<T> : Expression, IComparable<Expression>, IEquatable<Expression>, IEquatable<Expression<T>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Expression{T}"/> class.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="val">The value.</param>
        public Expression() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression{T}"/> class.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="val">The value.</param>
        public Expression(SourcePosition pos, T val) : base(pos)
        {
            SetValue(val);
        }

        public int CompareTo(Expression other)
        {
            return base.CompareTo(other);
        }

        #region Object Overrides

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(Expression<T> other)
        {
            return base.Equals(other);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion Object Overrides
    }
}