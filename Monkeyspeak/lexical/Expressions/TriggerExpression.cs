using Monkeyspeak.Lexical.Expressions;
using Monkeyspeak.Logging;
using Monkeyspeak.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Monkeyspeak.Lexical.Expressions
{
    internal class TriggerExpression : Expression<Trigger>
    {
        public TriggerExpression(SourcePosition pos, string value) : base(pos, ParseToTrigger(value, pos))
        {
        }

        private static Trigger ParseToTrigger(string value, SourcePosition pos)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Trigger.Undefined;
            }
            var cat = value.Substring(0, value.IndexOf(':'));
            if (string.IsNullOrWhiteSpace(cat))
            {
                return Trigger.Undefined;
            }
            var id = value.Substring(value.IndexOf(':') + 1);
            if (string.IsNullOrWhiteSpace(id))
            {
                return Trigger.Undefined;
            }
            return new Trigger((TriggerCategory)OtherUtils.IntParse(cat),
                OtherUtils.IntParse(id), pos);
        }

        public override bool Apply(Trigger? prevTrigger)
        {
            var currentTrigger = GetValue<Trigger>();
            if (prevTrigger.HasValue &&
                prevTrigger.Value.Category == TriggerCategory.Effect || prevTrigger.Value.Category == TriggerCategory.Flow &&
                currentTrigger.Category == TriggerCategory.Cause)
            {
                return false;
            }
            return true;
        }

        public override object Execute(Page page, Queue<IExpression> contents, bool addToPage = false)
        {
            return null;
        }
    }
}