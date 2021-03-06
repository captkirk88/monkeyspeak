﻿using Monkeyspeak.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Collections;

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
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
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
            get { return value; }
            set
            {
                // removed Value = as it interfered with page.setVariable - Gerolkae
                if (!CheckType(value)) throw new TypeNotSupportedException(value.GetType().Name +
                " is not a supported type. Expecting string, double or variable.");

                if (value != null && IsConstant)
                    throw new VariableIsConstantException($"Attempt to assign a value to constant '{Name}'");
                if (value is IVariable)
                    this.value = (value as IVariable).Value;
                else
                    this.value = value;
            }
        }

        public string Name { get; internal set; }

        public Variable(string name, bool constant = false)
        {
            IsConstant = constant;
            Name = name;
        }

        public Variable(string name, object value, bool constant = false)
        {
            IsConstant = constant;
            Name = name;
            this.value = value;
        }

        private bool CheckType(object _value)
        {
            if (_value == null) return true;

            return _value is string ||
                   _value is double ||
                   _value is IVariable;
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
        /// </summary>
        /// <param name="asConstant">Clone as Constant</param>
        /// <returns></returns>
        public Variable Clone(bool asConstant = false)
        {
            return new Variable(Name, value, asConstant);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="varA">The variable a.</param>
        /// <param name="varB">The variable b.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Variable varA, Variable varB)
        {
            return varA.Value == varB.Value;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="varA">The variable a.</param>
        /// <param name="varB">The variable b.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Variable varA, Variable varB)
        {
            return varA.Value != varB.Value;
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="varA">The variable a.</param>
        /// <param name="num"> The number.</param>
        /// <returns>The result of the operator.</returns>
        public static Variable operator +(Variable varA, double num)
        {
            varA.Value = varA.Value.AsDouble() + num;
            return varA;
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="varA">The variable a.</param>
        /// <param name="num"> The number.</param>
        /// <returns>The result of the operator.</returns>
        public static Variable operator -(Variable varA, double num)
        {
            varA.Value = varA.Value.AsDouble() - num;
            return varA;
        }

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="varA">The variable a.</param>
        /// <param name="num"> The number.</param>
        /// <returns>The result of the operator.</returns>
        public static Variable operator *(Variable varA, double num)
        {
            varA.Value = varA.Value.AsDouble() * num;
            return varA;
        }

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="varA">The variable a.</param>
        /// <param name="num"> The number.</param>
        /// <returns>The result of the operator.</returns>
        public static Variable operator /(Variable varA, double num)
        {
            varA.Value = varA.Value.AsDouble() / num;
            return varA;
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="varA">The variable a.</param>
        /// <param name="str"> The string.</param>
        /// <returns>The result of the operator.</returns>
        public static Variable operator +(Variable varA, string str)
        {
            varA.Value = varA.Value.AsDouble() + str;
            return varA;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Variable"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="var">The variable.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(Variable var)
        {
            return var.Value.AsString();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Variable"/> to <see cref="System.Double"/>.
        /// </summary>
        /// <param name="var">The variable.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator double(Variable var)
        {
            return var.Value.AsDouble();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj != null && obj is Variable && Equals((Variable)obj);
        }
    }

    [Serializable]
    [CLSCompliant(false)]
    public sealed class VariableTable : IVariable, IDictionary<string, object>
    {
        public static VariableTable Empty = new VariableTable("null", true, 0);

        private static int limit = 100;
        public static int Limit { get => limit; set => limit = value; }

        public string Name { get; private set; }

        /// <summary>
        /// Gets the index of the current element.
        /// </summary>
        /// <value>The index of the current element.</value>
        public int CurrentElementIndex { get; private set; }

        /// <summary>
        /// Gets or sets the active string based indexer.
        /// </summary>
        /// <value>The active indexer.</value>
        public string ActiveIndexer { get; set; }

        internal IDictionary<string, object> values;

        public object Value
        {
            get { return string.IsNullOrEmpty(ActiveIndexer) ? values.Values.FirstOrDefault() : this[ActiveIndexer]; }
            set
            {
                if (!CheckType(value)) throw new TypeNotSupportedException(value.GetType().Name +
                " is not a supported type. Expecting string, double or variable.");

                if (value != null && IsConstant)
                    throw new VariableIsConstantException($"Attempt to assign a value to constant '{Name}'");

                if (string.IsNullOrWhiteSpace(ActiveIndexer))
                    ActiveIndexer = "value";

                object newValue = null;
                if (value is IVariable var)
                    newValue = var.Value;
                else
                    newValue = value;

                this[ActiveIndexer] = newValue;
            }
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
                if (values.Count + 1 > Limit) return;

                if (!CheckType(value)) throw new TypeNotSupportedException(value.GetType().Name +
                " is not a supported type. Expecting string, double or variable.");

                if (value != null && IsConstant)
                    throw new VariableIsConstantException($"Attempt to assign a value to constant '{Name}'");
                object newValue = null;
                if (value is IVariable var)
                    newValue = var.Value;
                else
                    newValue = value;

                if (!values.ContainsKey(key))
                    values.Add(key, newValue);
                else values[key] = newValue;
            }
        }

        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                return At(index);
            }
        }

        public int Count { get => values.Count; }

        public bool IsConstant { get; private set; }

        public ICollection<string> Keys => new ReadOnlyCollection<string>(values.Keys.ToArray());

        public ICollection<object> Values => new ReadOnlyCollection<object>(values.Values.ToArray());

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => values.IsReadOnly;

        object IDictionary<string, object>.this[string key] { get => this[key]; set => this[key] = value; }

        public VariableTable(string name, bool isConstant = false, int limit = 100)
        {
            Name = name;
            values = new Dictionary<string, object>(limit);
            IsConstant = isConstant;
            Limit = limit;
        }

        public VariableTable(string name, IDictionary<string, object> dictionary, bool isConstant = false)
        {
            Name = name;
            values = dictionary;
            IsConstant = isConstant;
            Limit = limit;
        }

        public void Add(string key, object value)
        {
            if (values.Count + 1 > Limit) return;

            if (!CheckType(value)) throw new TypeNotSupportedException(value.GetType().Name +
            " is not a supported type. Expecting string, double or variable.");

            if (value != null && IsConstant)
                throw new VariableIsConstantException($"Attempt to assign a value to constant '{Name}'");

            object newValue = null;
            if (value is IVariable var)
                newValue = var.Value;
            else
                newValue = value;

            if (!values.ContainsKey(key))
                values.Add(key, newValue);
            else values[key] = newValue;
        }

        public void Add(object value)
        {
            if (values.Count + 1 > Limit) return;

            if (!CheckType(value)) throw new TypeNotSupportedException(value.GetType().Name +
            " is not a supported type. Expecting string, double or variable.");

            if (value != null && IsConstant)
                throw new VariableIsConstantException($"Attempt to assign a value to constant '{Name}'");
            var key = (values.Count + 1).ToString();

            object newValue = null;
            if (value is IVariable var)
                newValue = var.Value;
            else
                newValue = value;

            if (!values.ContainsKey(key))
                values.Add(key, newValue);
            else values[key] = newValue;
        }

        public void AddRange(params object[] args)
        {
            foreach (var arg in args) Add(arg);
        }

        public bool Contains(object value)
        {
            return values.Values.Contains(value);
        }

        private KeyValuePair<string, object> At(int index)
        {
            return values.ElementAtOrDefault(index);
        }

        public bool Next(out object value)
        {
            if (CurrentElementIndex > Count - 1)
            {
                CurrentElementIndex = 0;
                ActiveIndexer = null;
                value = null;
                return false;
            }
            var pair = this[CurrentElementIndex];
            CurrentElementIndex++;
            ActiveIndexer = pair.Key;
            value = pair.Value;
            return true;
        }

        private bool CheckType(object _value)
        {
            if (_value == null) return true;

            return _value is string ||
                   _value is double ||
                   _value is IVariable;
        }

        public bool Equals(IVariable other)
        {
            return Equals(values, other.Value) && string.Equals(Name, other.Name);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
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

        /// <summary>
        /// Clears all values in this table.
        /// </summary>
        public void Clear()
        {
            values.Clear();
        }

        public bool ContainsKey(string key)
        {
            return values.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return values.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return values.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return values.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            values.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return values.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public static VariableTable From(string name, IDictionary<string, object> dict)
        {
            var table = new VariableTable(name);
            foreach (var kv in dict) table.Add(kv);
            return table;
        }

        /// <summary>
        /// Index of key.
        /// </summary>
        /// <param name="searchKey">The search key.</param>
        /// <returns></returns>
        public int IndexOfKey(string searchKey)
        {
            int index = 0;
            foreach (var key in values.Keys)
            {
                if (key.Equals(searchKey, StringComparison.InvariantCulture))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        /// <summary>
        /// Index of value.
        /// </summary>
        /// <param name="searchValue">The search value.</param>
        /// <returns></returns>
        public int IndexOfValue(object searchValue)
        {
            if (!CheckType(searchValue)) return -1;

            int index = 0;
            foreach (var value in values.Values)
            {
                if (value.Equals(searchValue))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
    }
}