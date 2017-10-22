using Monkeyspeak.lexical.Expressions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Monkeyspeak
{
    [Serializable]
    public enum TriggerCategory : int
    {
        /// <summary>
        /// A trigger that was not defined.  You should never encounter this
        /// if you do then something isn't quite right.
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// A trigger defined with a 0
        /// <para>Example: (0:1) when someone says something, </para>
        /// </summary>
        Cause = 0,

        /// <summary>
        /// A trigger defined with a 1
        /// <para>Example: (1:2) and they moved # units left, </para>
        /// </summary>
        Condition = 1,

        /// <summary>
        /// A trigger defined with a 5
        /// <para>Example: (5:1) print {Hello World} to the console. </para>
        /// </summary>
        Effect = 5,

        /// <summary>
        /// A trigger defined with a 6
        /// <para>Example: (6:0) while variable % is #, </para>
        Flow = 6
    }

    [StructLayout(LayoutKind.Auto)]
    [Serializable]
    public struct Trigger : IEquatable<Trigger>
    {
        public static readonly Trigger Undefined = new Trigger(TriggerCategory.Undefined, -1);

        private TriggerCategory category;

        private int id;

        internal List<IExpression> contents;

        public Trigger(TriggerCategory cat, int id)
        {
            category = cat;
            this.id = id;
            contents = new List<IExpression>();
        }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public TriggerCategory Category
        {
            get { return category; }
            internal set { category = value; }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id
        {
            get { return id; }
            internal set { id = value; }
        }

        public IReadOnlyCollection<IExpression> Contents
        {
            get { return contents.AsReadOnly(); }
            set { contents.AddRange(value); }
        }

        internal Trigger Clone()
        {
            var clone = new Trigger(category, id)
            {
                contents = new List<IExpression>(contents)
            };
            return clone;
        }

        public static bool operator ==(Trigger a, Trigger b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Trigger a, Trigger b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Trigger other)
        {
            if (other == null) return false;
            return other.category == category && other.id == id;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Trigger)
            {
                var other = (Trigger)obj;
                return other != Undefined && other.category == category && other.id == id;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return category.GetHashCode() + id.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"({(int)category}:{id})";
    }
}