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
    ///
    /// </summary>
    /// <seealso cref="Monkeyspeak.Libraries.BaseLibrary" />
    public sealed class Loops : AutoIncrementBaseLibrary
    {
        public override int BaseId => 450;

        public override void Initialize()
        {
            Add(TriggerCategory.Flow, WhileVarIsNotValue,
                "while variable % is not #,");

            Add(TriggerCategory.Flow, WhileVarIsValue,
                "while variable % is #,");

            Add(TriggerCategory.Flow, WhileVarIsNotString,
                "while variable % is not {...},");

            Add(TriggerCategory.Flow, WhileVarIsString,
                "while variable % is {...},");

            Add(TriggerCategory.Flow, AfterLoopIsDone,
                "after the loop is done,");

            Add(TriggerCategory.Effect, BreakCurrentFlow,
                "exit the current loop.");
        }

        private bool AfterLoopIsDone(TriggerReader reader)
        {
            bool canContinue = true;
            if (!reader.Page.HasVariable("___after_loop", out ConstantVariable counter))
                counter = reader.Page.SetVariable(new ConstantVariable("___after_loop", 0d));
            else counter.SetValue(counter.Value.As<double>() + 1d);
            if (counter.Value.As<double>() == 1)
            {
                canContinue = false;
                reader.Page.RemoveVariable(counter);
            }
            return canContinue;
        }

        private bool BreakCurrentFlow(TriggerReader reader)
        {
            reader.CurrentBlockIndex = -1;
            return true;
        }

        private bool WhileVarIsString(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            var value = reader.ReadString();
            bool canContinue = var.Value.As<string>().Equals(value, StringComparison.InvariantCulture);

            if (!reader.Page.HasVariable("___while_counter", out ConstantVariable whileCounter))
                whileCounter = reader.Page.SetVariable(new ConstantVariable("___while_counter", 0d));
            whileCounter.SetValue(whileCounter.Value.As<double>() + 1);
            if (whileCounter.Value.As<double>() >= reader.Engine.Options.LoopLimit)
            {
                canContinue = false;
            }

            if (!canContinue)
            {
                reader.Page.RemoveVariable(whileCounter);
            }
            return canContinue;
        }

        private bool WhileVarIsNotString(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            var value = reader.ReadString();
            bool canContinue = !var.Value.As<string>().Equals(value, StringComparison.InvariantCulture);
            if (!reader.Page.HasVariable("___while_counter", out ConstantVariable whileCounter))
                whileCounter = reader.Page.SetVariable(new ConstantVariable("___while_counter", 0d));
            whileCounter.SetValue(whileCounter.Value.As<double>() + 1);
            if (whileCounter.Value.As<double>() >= reader.Engine.Options.LoopLimit)
            {
                canContinue = false;
            }

            if (!canContinue)
            {
                reader.Page.RemoveVariable(whileCounter);
            }
            return canContinue;
        }

        private bool WhileVarIsValue(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            var value = reader.ReadNumber();
            var varVal = var.Value.As<double>();
            bool canContinue = varVal == value;

            if (!reader.Page.HasVariable("___while_counter", out ConstantVariable whileCounter))
                whileCounter = reader.Page.SetVariable(new ConstantVariable("___while_counter", 0d));
            whileCounter.SetValue(whileCounter.Value.As<double>() + 1);
            if (whileCounter.Value.As<double>() >= reader.Engine.Options.LoopLimit)
            {
                canContinue = false;
            }

            if (!canContinue)
            {
                reader.Page.RemoveVariable(whileCounter);
            }
            return canContinue;
        }

        private bool WhileVarIsNotValue(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            var value = reader.ReadNumber();
            bool canContinue = var.Value.As<double>() != value;
            if (!reader.Page.HasVariable("___while_counter", out ConstantVariable whileCounter))
                whileCounter = reader.Page.SetVariable(new ConstantVariable("___while_counter", 0d));
            whileCounter.SetValue(whileCounter.Value.As<double>() + 1);
            if (whileCounter.Value.As<double>() >= reader.Engine.Options.LoopLimit)
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