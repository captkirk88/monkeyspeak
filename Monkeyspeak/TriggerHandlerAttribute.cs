using Monkeyspeak.Libraries;
using System;
using System.Reflection;

namespace Monkeyspeak
{
    /// <summary>
    ///
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class TriggerHandlerAttribute : Attribute
    {
        private TriggerCategory triggerCategory;
        private int triggerID;
        private string description;
        internal MethodInfo owner;

        public string Description { get => description; set => description = value; }
        public int TriggerID { get => triggerID; set => triggerID = value; }
        public TriggerCategory TriggerCategory { get => triggerCategory; set => triggerCategory = value; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="category">Trigger Category</param>
        /// <param name="id">Trigger ID</param>
        /// <param name="description">Trigger Description</param>
        public TriggerHandlerAttribute(TriggerCategory category, int id, string description)
        {
            this.TriggerCategory = category;
            this.TriggerID = id;
            this.Description = description;
        }

        internal void Register(Page page)
        {
            var handler = (TriggerHandler)(reader => (bool)owner.Invoke(null, new object[] { reader }));
            if (handler != null)
            {
                Attributes.Instance.Add(new Trigger(TriggerCategory, TriggerID), handler, Description);
            }
        }
    }
}