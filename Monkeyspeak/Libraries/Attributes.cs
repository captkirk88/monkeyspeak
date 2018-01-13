using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Libraries
{
    /// <summary>
    /// Specifically made for the Monkeyspeak Editor.
    ///
    /// Any description you provide will be used in the intellisense of the editor.
    /// Add it to your TriggerHandler methods.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class TriggerDescriptionAttribute : Attribute
    {
        private readonly string description;

        public TriggerDescriptionAttribute(string description)
        {
            this.description = description;
        }

        public string Description
        {
            get { return description; }
        }
    }

    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class TriggerParameterAttribute : Attribute
    {
        private readonly string description;

        public TriggerParameterAttribute(string description)
        {
            this.description = description;
        }

        public string Description
        {
            get { return description; }
        }
    }

    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class TriggerVariableParameterAttribute : TriggerParameterAttribute
    {
        public TriggerVariableParameterAttribute() : base("Variable or table")
        {
        }
    }

    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class TriggerNumberParameterAttribute : TriggerParameterAttribute
    {
        public TriggerNumberParameterAttribute() : base("Number or variable with number in it")
        {
        }
    }

    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class TriggerStringParameterAttribute : TriggerParameterAttribute
    {
        public TriggerStringParameterAttribute() : base("String containing words, variables and/or numbers")
        {
        }
    }
}