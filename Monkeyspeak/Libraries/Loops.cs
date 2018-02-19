using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Libraries
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Monkeyspeak.Libraries.BaseLibrary"/>
    public sealed class Loops : AutoIncrementBaseLibrary
    {
        public override int BaseId => 450;

        public override void Initialize(params object[] args)
        {
            Add(TriggerCategory.Flow, WhileVarIsNotValue,
                "while variable % is not #,");

            Add(TriggerCategory.Flow, WhileVarIsValue,
                "while variable % is #,");

            Add(TriggerCategory.Flow, WhileVarIsGreaterThanValue,
                "while variable % is greater than #,");

            Add(TriggerCategory.Flow, WhileVarIsLessThanValue,
                "while variable % is less than #,");

            Add(TriggerCategory.Flow, WhileVarIsNotString,
                "while variable % is not {...},");

            Add(TriggerCategory.Flow, WhileVarIsString,
                "while variable % is {...},");

            Add(TriggerCategory.Flow, RepeatNumTimes,
                "repeat # times,");

            Add(TriggerCategory.Flow, AfterLoopIsDone,
                "after the loop is done,");

            Add(TriggerCategory.Effect, BreakCurrentFlow,
                "exit the current loop.");
        }

        private bool RepeatNumTimes(TriggerReader reader)
        {
            var num = reader.ReadNumber();

            if (!reader.Page.HasVariable("___repeat_counter", out ConstantVariable repeatCounter))
                repeatCounter = reader.Page.SetVariable(new ConstantVariable("___repeat_counter", 0d));
            if (!reader.Page.HasVariable("___repeat_counter_max", out ConstantVariable repeatCounterMax))
                repeatCounterMax = reader.Page.SetVariable(new ConstantVariable("___repeat_counter_max", num));
            repeatCounter.SetValue(repeatCounter.Value.AsDouble() + 1);
            bool canContinue = repeatCounter.Value.AsDouble() < repeatCounterMax.Value.AsDouble();
            if (repeatCounter.Value.AsDouble() >= reader.Engine.Options.LoopLimit)
            {
                canContinue = false;
            }

            if (!canContinue)
            {
                reader.Page.RemoveVariable(repeatCounter);
                reader.Page.RemoveVariable(repeatCounterMax);
            }
            return canContinue;
        }

        private bool WhileVarIsLessThanValue(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            var value = reader.ReadNumber();
            var varVal = var.Value.AsDouble();
            bool canContinue = varVal < value;

            if (!reader.Page.HasVariable("___while_counter", out ConstantVariable whileCounter))
                whileCounter = reader.Page.SetVariable(new ConstantVariable("___while_counter", 0d));
            whileCounter.SetValue(whileCounter.Value.AsDouble() + 1);
            if (whileCounter.Value.AsDouble() >= reader.Engine.Options.LoopLimit)
            {
                canContinue = false;
            }

            if (!canContinue)
            {
                reader.Page.RemoveVariable(whileCounter);
            }
            return canContinue;
        }

        private bool WhileVarIsGreaterThanValue(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            var value = reader.ReadNumber();
            var varVal = var.Value.AsDouble();
            bool canContinue = varVal > value;

            if (!reader.Page.HasVariable("___while_counter", out ConstantVariable whileCounter))
                whileCounter = reader.Page.SetVariable(new ConstantVariable("___while_counter", 0d));
            whileCounter.SetValue(whileCounter.Value.AsDouble() + 1);
            if (whileCounter.Value.AsDouble() >= reader.Engine.Options.LoopLimit)
            {
                canContinue = false;
            }

            if (!canContinue)
            {
                reader.Page.RemoveVariable(whileCounter);
            }
            return canContinue;
        }

        [TriggerDescription("Executed when the loop completes or is exited")]
        private bool AfterLoopIsDone(TriggerReader reader)
        {
            bool canContinue = true;
            if (!reader.Page.HasVariable("___after_loop", out ConstantVariable counter))
                counter = reader.Page.SetVariable(new ConstantVariable("___after_loop", 0d));
            else counter.SetValue(counter.Value.AsDouble() + 1d);
            if (counter.Value.AsDouble() >= 1)
            {
                canContinue = false;
                foreach (var var in reader.Page.Scope)
                {
                    if (var.Name.StartsWith("___") && var.IsConstant)
                        reader.Page.RemoveVariable(var);
                }
            }
            return canContinue;
        }

        [TriggerDescription("Exits the current loop")]
        private bool BreakCurrentFlow(TriggerReader reader)
        {
            reader.CurrentBlockIndex = -2;
            foreach (var var in reader.Page.Scope)
            {
                if (var.Name.StartsWith("___") && var.IsConstant)
                    reader.Page.RemoveVariable(var);
            }
            return true;
        }

        [TriggerDescription("Loops through the block below this trigger while the variable equals the specified string")]
        [TriggerVariableParameter]
        [TriggerStringParameter]
        private bool WhileVarIsString(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            var value = reader.ReadString();
            bool canContinue = var.Value.AsString().Equals(value, StringComparison.InvariantCulture);

            if (!reader.Page.HasVariable("___while_counter", out ConstantVariable whileCounter))
                whileCounter = reader.Page.SetVariable(new ConstantVariable("___while_counter", 0d));
            whileCounter.SetValue(whileCounter.Value.AsDouble() + 1);
            if (whileCounter.Value.AsDouble() >= reader.Engine.Options.LoopLimit)
            {
                canContinue = false;
            }

            if (!canContinue)
            {
                reader.Page.RemoveVariable(whileCounter);
            }
            return canContinue;
        }

        [TriggerDescription("Loops through the block below this trigger while the variable does not equal the specified string")]
        [TriggerVariableParameter]
        [TriggerStringParameter]
        private bool WhileVarIsNotString(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            var value = reader.ReadString();
            bool canContinue = !var.Value.AsString().Equals(value, StringComparison.InvariantCulture);
            if (!reader.Page.HasVariable("___while_counter", out ConstantVariable whileCounter))
                whileCounter = reader.Page.SetVariable(new ConstantVariable("___while_counter", 0d));
            whileCounter.SetValue(whileCounter.Value.AsDouble() + 1);
            if (whileCounter.Value.AsDouble() >= reader.Engine.Options.LoopLimit)
            {
                canContinue = false;
            }

            if (!canContinue)
            {
                reader.Page.RemoveVariable(whileCounter);
            }
            return canContinue;
        }

        [TriggerDescription("Loops through the block below this trigger while the variable equals the specified number")]
        [TriggerVariableParameter]
        [TriggerNumberParameter]
        private bool WhileVarIsValue(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            var value = reader.ReadNumber();
            var varVal = var.Value.AsDouble();
            bool canContinue = varVal == value;

            if (!reader.Page.HasVariable("___while_counter", out ConstantVariable whileCounter))
                whileCounter = reader.Page.SetVariable(new ConstantVariable("___while_counter", 0d));
            whileCounter.SetValue(whileCounter.Value.AsDouble() + 1);
            if (whileCounter.Value.AsDouble() >= reader.Engine.Options.LoopLimit)
            {
                canContinue = false;
            }

            if (!canContinue)
            {
                reader.Page.RemoveVariable(whileCounter);
            }
            return canContinue;
        }

        [TriggerDescription("Loops through the block below this trigger while the variable does not equal the specified number")]
        [TriggerVariableParameter]
        [TriggerNumberParameter]
        private bool WhileVarIsNotValue(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            var value = reader.ReadNumber();
            bool canContinue = var.Value.AsDouble() != value;
            if (!reader.Page.HasVariable("___while_counter", out ConstantVariable whileCounter))
                whileCounter = reader.Page.SetVariable(new ConstantVariable("___while_counter", 0d));
            whileCounter.SetValue(whileCounter.Value.AsDouble() + 1);
            if (whileCounter.Value.AsDouble() >= reader.Engine.Options.LoopLimit)
            {
                canContinue = false;
            }
            if (!canContinue)
            {
                reader.Page.RemoveVariable(whileCounter);
            }
            return canContinue;
        }

        public override void Unload(Page page)
        {
            foreach (var var in page.Scope)
            {
                if (var.Name.Contains("___while_counter") || var.Name.Contains("___after_loop"))
                    page.RemoveVariable(var);
            }
        }
    }
}