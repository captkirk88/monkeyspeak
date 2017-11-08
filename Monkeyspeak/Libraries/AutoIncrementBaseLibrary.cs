using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Libraries
{
    /// <summary>
    /// Automatically increments the Trigger Id for each Trigger category so that you don't have to deal with it!
    /// </summary>
    /// <seealso cref="Monkeyspeak.Libraries.BaseLibrary" />
    public abstract class AutoIncrementBaseLibrary : BaseLibrary
    {
        private int triggerCauseIdCounter = 0, triggerConditionIdCounter = 0, triggerEffectIdCounter = 0, triggerFlowIdCounter = 0;

        public abstract int BaseId { get; }

        /// <summary>
        /// Registers a Trigger to the TriggerHandler with optional description
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="handler"></param>
        /// <param name="description"></param>
        public override void Add(Trigger trigger, TriggerHandler handler, string description = null)
        {
            if (description != null && !descriptions.ContainsKey(trigger)) descriptions.Add(trigger, description);
            if (!handlers.ContainsKey(trigger))
            {
                switch (trigger.Category)
                {
                    case TriggerCategory.Cause:
                        triggerCauseIdCounter++;
                        break;

                    case TriggerCategory.Condition:
                        triggerConditionIdCounter++;
                        break;

                    case TriggerCategory.Effect:
                        triggerEffectIdCounter++;
                        break;

                    case TriggerCategory.Flow:
                        triggerFlowIdCounter++;
                        break;
                }
                handlers.Add(trigger, handler);
            }
            else throw new UnauthorizedAccessException($"Override of existing Trigger {trigger}'s handler with handler in {handler.Method}.");
        }

        public override void Add(TriggerCategory cat, int id, TriggerHandler handler, string description = null)
        {
            Trigger trigger = new Trigger(cat, id);
            if (description != null) descriptions.Add(trigger, description);
            if (!handlers.ContainsKey(trigger))
            {
                switch (trigger.Category)
                {
                    case TriggerCategory.Cause:
                        triggerCauseIdCounter++;
                        break;

                    case TriggerCategory.Condition:
                        triggerConditionIdCounter++;
                        break;

                    case TriggerCategory.Effect:
                        triggerEffectIdCounter++;
                        break;

                    case TriggerCategory.Flow:
                        triggerFlowIdCounter++;
                        break;
                }
                handlers.Add(trigger, handler);
            }
            else throw new UnauthorizedAccessException($"Override of existing Trigger {trigger}'s handler with handler in {handler.Method}.");
        }

        /// <summary>
        /// Registers a Trigger to the TriggerHandler with optional description
        /// </summary>
        /// <param name="cat"></param>
        /// <param name="id"></param>
        /// <param name="handler"></param>
        /// <param name="description"></param>
        public void Add(TriggerCategory cat, TriggerHandler handler, string description = null)
        {
            int id = BaseId;
            switch (cat)
            {
                case TriggerCategory.Cause:
                    id += triggerCauseIdCounter++;
                    break;

                case TriggerCategory.Condition:
                    id += triggerConditionIdCounter++;
                    break;

                case TriggerCategory.Effect:
                    id += triggerEffectIdCounter++;
                    break;

                case TriggerCategory.Flow:
                    id += triggerFlowIdCounter++;
                    break;
            }

            Trigger trigger = new Trigger(cat, id);
            if (description != null) descriptions.Add(trigger, description);
            if (!handlers.ContainsKey(trigger))
                handlers.Add(trigger, handler);
            // the below should never happen with this approach provided the BaseId is a unassigned trigger id
            else throw new UnauthorizedAccessException($"Override of existing Trigger {trigger}'s handler with handler in {handler.Method}.");
        }
    }
}