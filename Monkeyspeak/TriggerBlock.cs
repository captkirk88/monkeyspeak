using System;
using System.Collections.Generic;
using System.Text;

namespace Monkeyspeak
{
    [Serializable]
    public class TriggerBlock : List<Trigger>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerBlock"/> class.
        /// </summary>
        public TriggerBlock()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerBlock"/> class.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity.</param>
        public TriggerBlock(int initialCapacity) :
            base(initialCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerBlock"/> class.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        public TriggerBlock(IEnumerable<Trigger> collection) : base(collection)
        {
        }

        /// <summary>
        /// Operates like IndexOf for Triggers
        /// </summary>
        /// <param name="cat"></param>
        /// <param name="id"></param>
        /// <param name="startIndex"></param>
        /// <returns>Index of trigger or -1 if not found</returns>
        public int IndexOfTrigger(TriggerCategory cat, int id = -1, int startIndex = 0)
        {
            if (startIndex < Count)
                for (int i = startIndex; i <= Count - 1; i++)
                {
                    Trigger trigger = base[i];
                    if (trigger.Category == cat)
                    {
                        if (id == -1 || trigger.Id == id)
                            return i;
                    }
                }
            return -1;
        }

        public int LastIndexOfTrigger(TriggerCategory cat, int id = -1, int index = 0)
        {
            int lastIndex = -1;
            for (int i = index; i <= Count - 1; i++)
            {
                Trigger trigger = base[i];
                if (trigger.Category == cat)
                {
                    if (id == -1 || trigger.Id == id)
                        lastIndex = i;
                }
            }
            return lastIndex;
        }

        /// <summary>
        /// Determines whether the block contains the trigger.
        /// </summary>
        /// <param name="cat">The category.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="index">Index in the block to start from</param>
        /// <returns>
        ///   <c>true</c> if the block contains the trigger; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsTrigger(TriggerCategory cat, int id = -1, int index = 0)
        {
            if (index < Count)
                for (int i = index; i <= Count - 1; i++)
                {
                    Trigger trigger = base[i];
                    if (trigger.Category == cat)
                    {
                        if (id == -1 || trigger.Id == id)
                            return true;
                    }
                }
            return false;
        }

        /// <summary>
        /// Creates a sub block.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count. Value of less than 0 will go to the end of the collection</param>
        /// <returns></returns>
        public TriggerBlock GetSubBlock(int index, int count = -1)
        {
            if (index < 0) index = 0;
            if (count > Count) count = Count;
            return new TriggerBlock(GetRange(index, count < 0 ? Count - index : count - index));
        }

        public string ToString(char separator)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= Count - 1; i++)
            {
                sb.Append(this[i]);
                if (i != Count) sb.Append(separator);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return base.ToString();
        }
    }
}