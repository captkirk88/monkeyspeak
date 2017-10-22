﻿using Monkeyspeak.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;

namespace Monkeyspeak
{
    [Serializable]
    public class VariableIsConstantException : Exception
    {
        public VariableIsConstantException()
        {
        }

        public VariableIsConstantException(string message)
            : base(message)
        {
        }

        public VariableIsConstantException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected VariableIsConstantException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    public class VariableEqualityComparer : IEqualityComparer<IVariable>
    {
        public bool Equals(IVariable x, IVariable y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(IVariable obj)
        {
            return obj.GetHashCode();
        }
    }

    public interface IVariable : IEquatable<IVariable>
    {
        string Name { get; }
        object Value { get; set; }
        bool IsConstant { get; }
    }

    [Serializable]
    [CLSCompliant(false)]
    public class Variable : IVariable
    {
        public bool Equals(IVariable other)
        {
            return Equals(value, other.Value) && string.Equals(Name, other.Name);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return Name.GetHashCode();
            }
        }

        public static readonly IVariable NoValue = new Variable("%none", null, true);

        public bool IsConstant
        {
            get;
            private set;
        }

        private object value;

        public object Value
        {
            get { return value ?? "null"; }
            set
            {
                // removed Value = as it interfered with page.setVariable - Gerolkae
                if (!CheckType(value)) throw new TypeNotSupportedException(value.GetType().Name +
                " is not a supported type. Expecting string or double.");

                if (value != null && IsConstant)
                    throw new VariableIsConstantException($"Attempt to assign a value to constant '{Name}'");
                this.value = value;
            }
        }

        public string Name { get; internal set; }

        internal Variable(string name, bool constant = false)
        {
            IsConstant = constant;
            Name = name;
        }

        internal Variable(string name, object value, bool constant = false)
        {
            IsConstant = constant;
            Name = name;
            this.value = value;
        }

        private bool CheckType(object _value)
        {
            if (_value == null) return true;

            return _value is string ||
                   _value is double;
        }

        /// <summary>
        /// Returns a const identifier if the variable is constant followed by name,
        /// <para>otherwise just the name is returned.</para>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ((IsConstant) ? "const " : "") + $"{Name} = {value ?? "null"}";
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="asConstant">Clone as Constant</param>
        /// <returns></returns>
        public Variable Clone(bool asConstant = false)
        {
            return new Variable(Name, value, asConstant);
        }

        public static bool operator ==(Variable varA, Variable varB)
        {
            return varA.Value == varB.Value;
        }

        public static bool operator !=(Variable varA, Variable varB)
        {
            return varA.Value != varB.Value;
        }

        public static Variable operator +(Variable varA, double num)
        {
            varA.Value = varA.Value.As<double>() + num;
            return varA;
        }

        public static Variable operator -(Variable varA, double num)
        {
            varA.Value = varA.Value.As<double>() - num;
            return varA;
        }

        public static Variable operator *(Variable varA, double num)
        {
            varA.Value = varA.Value.As<double>() * num;
            return varA;
        }

        public static Variable operator /(Variable varA, double num)
        {
            varA.Value = varA.Value.As<double>() / num;
            return varA;
        }

        public static Variable operator +(Variable varA, string str)
        {
            varA.Value = varA.Value.As<string>() + str;
            return varA;
        }

        public static implicit operator string(Variable var)
        {
            return var.Value.As<string>();
        }

        public static implicit operator double(Variable var)
        {
            return var.Value.As<double>();
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is Variable && Equals((Variable)obj);
        }
    }

    [Serializable]
    [CLSCompliant(false)]
    public sealed class VariableTable : IVariable
    {
        private static int limit = 100;
        public static int Limit { get => limit; set => limit = value; }

        public string Name { get; private set; }

        /// <summary>
        /// Gets the index of the current element.
        /// </summary>
        /// <value>
        /// The index of the current element.
        /// </value>
        public int CurrentElementIndex { get; private set; }

        /// <summary>
        /// Gets or sets the active string based indexer.
        /// </summary>
        /// <value>
        /// The active indexer.
        /// </value>
        public string ActiveIndexer { get; set; }

        internal Dictionary<string, object> values;

        public object Value
        {
            get { return values.LastOrDefault(); }
            set { this[ActiveIndexer] = value; }
        }

        public object this[string key]
        {
            get
            {
                if (values.TryGetValue(key, out object value))
                    return value;
                return null;
            }
            set
            {
                if (!CheckType(value) || values.Count + 1 > Limit) return;
                values[key] = value;
            }
        }

        public object this[int index]
        {
            get
            {
                return At(index);
            }
        }

        public int Count { get => values.Count; }

        public bool IsConstant { get; private set; }

        public IReadOnlyDictionary<string, object> Contents
        {
            get => new ReadOnlyDictionary<string, object>(values);
        }

        public VariableTable(string name, bool isConstant = false, int limit = 100)
        {
            Name = name;
            values = new Dictionary<string, object>(limit);
            IsConstant = isConstant;
            Limit = limit;
        }

        public void Add(string key, object value)
        {
            if (CheckType(value) || values.Count > Limit) return;
            values[key] = value;
        }

        public bool Contains(string index)
        {
            return values.ContainsKey(index);
        }

        private object At(int index)
        {
            return values.Values.ElementAtOrDefault(index);
        }

        public object Next()
        {
            var obj = this[CurrentElementIndex];
            CurrentElementIndex++;
            return obj;
        }

        private bool CheckType(object _value)
        {
            if (_value == null) return true;

            return _value is string ||
                   _value is double;
        }

        public bool Equals(IVariable other)
        {
            return Equals(values, other.Value) && string.Equals(Name, other.Name);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return Name.GetHashCode();
            }
        }

        /// <summary>
        /// Returns a const identifier if the variable is constant followed by name,
        /// <para>otherwise just the name is returned.</para>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string replace = (values != null ? values.ToString(',') : "null");
            return ((IsConstant) ? "const " : "") + $"{Name} = {replace}";
        }

        public void ResetIndex()
        {
            CurrentElementIndex = 0;
        }
    }
}