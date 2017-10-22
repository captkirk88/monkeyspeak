using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Libraries
{
    public class Attributes : BaseLibrary
    {
        public static readonly Attributes Instance = new Attributes();

        private Attributes()
        {
        }

        public override void Initialize()
        {
        }

        public void AddDescription(Trigger trigger, string description)
        {
            if (string.IsNullOrEmpty(description)) return;
            if (!descriptions.ContainsKey(trigger))
                descriptions.Add(trigger, description);
        }

        public new void Add(Trigger trigger, TriggerHandler handler, string description = null)
        {
            if (description != null && !descriptions.ContainsKey(trigger)) descriptions.Add(trigger, description);
            if (!handlers.ContainsKey(trigger))
                handlers.Add(trigger, handler);
        }

        public new void Add(TriggerCategory cat, int id, TriggerHandler handler, string description = null)
        {
            var trigger = new Trigger(cat, id);
            if (description != null && !descriptions.ContainsKey(trigger)) descriptions.Add(trigger, description);
            if (!handlers.ContainsKey(trigger))
                handlers.Add(trigger, handler);
        }

        public override void Unload(Page page)
        {
        }
    }
}