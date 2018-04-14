using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using Monkeyspeak.Utils;

namespace Monkeyspeak
{
    public class ObjectVariable : IVariable
    {
        public static readonly ObjectVariable Null = new ObjectVariable("%null");

        private dynamic wrappedObject;

        public ObjectVariable(string name)
        {
            Name = name;
            wrappedObject = new ExpandoObject();
        }

        public ObjectVariable(string name, object wrappedObject)
        {
            Name = name;
            this.wrappedObject = wrappedObject ?? new ExpandoObject();
        }

        public ObjectVariable(string name, IDictionary<string, object> content)
        {
            wrappedObject = new ExpandoObject();
            var coll = (ICollection<KeyValuePair<string, object>>)wrappedObject;
            foreach (var pair in content)
            {
                coll.Add(pair);
            }
        }

        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the desired property to lookup.
        /// </summary>
        /// <value>The desired property.</value>
        public string DesiredProperty { get; set; }

        /// <summary>
        /// Gets the dynamic value.
        /// </summary>
        /// <value>The dynamic value.</value>
        public dynamic DynamicValue
        {
            get => wrappedObject;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value
        {
            get
            {
                if (wrappedObject == null) return null;
                if (DesiredProperty.IsNullOrBlank())
                {
                    try
                    {
                        if (CheckType(wrappedObject.Value))
                            return wrappedObject.Value;
                        else return null;
                    }
                    catch (RuntimeBinderException) { return null; }
                }
                object value = null;
                if (!(wrappedObject is ExpandoObject))
                    value = ReflectionHelper.GetPropertyValue(wrappedObject, DesiredProperty);
                else
                {
                    IDictionary<string, object> dynDict = wrappedObject;
                    dynDict.TryGetValue(DesiredProperty, out value);
                }
                if (CheckType(value))
                    return value;
                return null;
            }
            set
            {
                if (CheckType(value))
                {
                    if (wrappedObject is ExpandoObject)
                    {
                        if (!DesiredProperty.IsNullOrBlank())
                        {
                            var dynDict = wrappedObject as IDictionary<string, object>;
                            dynDict[DesiredProperty] = value;
                        }
                        else wrappedObject.Value = value;
                    }
                    else ReflectionHelper.SetPropertyValue(wrappedObject, DesiredProperty, value);
                }
            }
        }

        public VariableTable ConvertToTable()
        {
            VariableTable table = new VariableTable(Name);
            if (wrappedObject.GetType() == typeof(ExpandoObject))
            {
                IDictionary<string, object> dynDict = wrappedObject;
                foreach (var pair in dynDict)
                {
                    table.Add(pair);
                }
            }

            return table;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is constant.
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsConstant { get => false; }

        private bool CheckType(object _value)
        {
            if (_value == null) return true;

            bool result = _value is string ||
                   _value is double ||
                   _value is IVariable;
            if (!result) throw new TypeNotSupportedException($"{_value.GetType().Name} is not a supported type. Expecting string, double or variable.");
            return result;
        }

        public override string ToString()
        {
            if (wrappedObject is ExpandoObject)
            {
                IDictionary<string, object> dynDict = wrappedObject;
                string replace = dynDict.ToString(',');
                return $"{Name} = {replace}";
            }
            return $"{Name}{(DesiredProperty.IsNullOrBlank() ? string.Empty : "." + DesiredProperty)}";
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(IVariable other) => Equals(Value, other.Value) && string.Equals(Name, other.Name);

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj != null && obj is Variable && Equals((Variable)obj);
    }
}