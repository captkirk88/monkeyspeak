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
    [System.AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
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
}