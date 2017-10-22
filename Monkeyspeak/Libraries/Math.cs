using Monkeyspeak.Extensions;
using System;

namespace Monkeyspeak.Libraries
{
    public class Math : BaseLibrary
    {
        public override void Initialize()
        {
            // (1:150) and variable % is greater than #,
            Add(new Trigger(TriggerCategory.Condition, 150), VariableGreaterThan,
                "and variable % is greater than #,");
            // (1:151) and variable % is greater than or equal to #,
            Add(new Trigger(TriggerCategory.Condition, 151), VariableGreaterThanOrEqual,
                "and variable % is greater than or equal to #,");

            // (1:152) and variable % is less than #,
            Add(new Trigger(TriggerCategory.Condition, 152), VariableLessThan,
                "and variable % is less than #,");

            // (1:153) and variable % is less than or equal to #,
            Add(new Trigger(TriggerCategory.Condition, 153), VariableLessThanOrEqual,
                "and variable % is less than or equal to #,");

            // (5:150) take variable % and add # to it.
            Add(new Trigger(TriggerCategory.Effect, 150), AddToVariable,
                "take variable % and add # to it.");

            // (5:151) take variable % and substract it by #.
            Add(new Trigger(TriggerCategory.Effect, 151), SubtractFromVariable,
                "take variable % and subtract # from it.");

            // (5:152) take variable % and multiply it by #.
            Add(new Trigger(TriggerCategory.Effect, 152), MultiplyByVariable,
                "take variable % and multiply it by #.");

            // (5:153) take variable % and divide it by #.
            Add(new Trigger(TriggerCategory.Effect, 153), MultiplyByVariable,
                "take variable % and divide it by #.");
        }

        public override void Unload(Page page)
        {
        }

        private bool AddToVariable(TriggerReader reader)
        {
            var toAssign = reader.ReadVariable(true);
            double num = reader.ReadVariableOrNumber();

            toAssign.Value = toAssign.Value.As<double>() + num;
            return true;
        }

        private bool DivideByVariable(TriggerReader reader)
        {
            var toAssign = reader.ReadVariable(true);
            double num = reader.ReadVariableOrNumber();

            toAssign.Value = toAssign.Value.As<double>() / num;
            return true;
        }

        private bool MultiplyByVariable(TriggerReader reader)
        {
            var toAssign = reader.ReadVariable(true);
            double num = reader.ReadVariableOrNumber();

            toAssign.Value = toAssign.Value.As<double>() * num;
            return true;
        }

        private bool SubtractFromVariable(TriggerReader reader)
        {
            var toAssign = reader.ReadVariable(true);
            double num = reader.ReadVariableOrNumber();

            toAssign.Value = toAssign.Value.As<double>() - num;
            return true;
        }

        private bool VariableGreaterThan(TriggerReader reader)
        {
            var mainVar = reader.ReadVariable();
            double num = reader.ReadVariableOrNumber();
            return mainVar.Value.As<double>() > num;
        }

        private bool VariableGreaterThanOrEqual(TriggerReader reader)
        {
            var mainVar = reader.ReadVariable();
            double num = reader.ReadVariableOrNumber();
            return mainVar.Value.As<double>() >= num;
        }

        private bool VariableLessThan(TriggerReader reader)
        {
            var mainVar = reader.ReadVariable();
            double num = reader.ReadVariableOrNumber();
            return mainVar.Value.As<double>() < num;
        }

        private bool VariableLessThanOrEqual(TriggerReader reader)
        {
            var mainVar = reader.ReadVariable();
            double num = reader.ReadVariableOrNumber();
            return mainVar.Value.As<double>() <= num;
        }
    }
}