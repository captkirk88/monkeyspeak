using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using System;
using System.Linq;

namespace Monkeyspeak.Libraries
{
    public class Sys : BaseLibrary
    {
        public override void Initialize(params object[] args)
        {
            // (1:100) and variable % is defined,
            Add(new Trigger(TriggerCategory.Condition, 100), IsVariableDefined,
                "and variable % is defined,");

            // (1:101) and variable % is not defined,
            Add(new Trigger(TriggerCategory.Condition, 101), IsVariableNotDefined,
                "and variable % is not defined,");

            // (1:102) and variable % equals #,
            Add(new Trigger(TriggerCategory.Condition, 102), IsVariableEqualToNumberOrVar,
                "and variable % equals #,");

            // (1:103) and variable % does not equal #,
            Add(new Trigger(TriggerCategory.Condition, 103), IsVariableNotEqualToNumberOrVar,
                "and variable % does not equal #,");

            // (1:104) and variable % equals {...},
            Add(new Trigger(TriggerCategory.Condition, 104), IsVariableEqualToString,
                "and variable % equals {...},");

            // (1:105) and variable % does not equal {...},
            Add(new Trigger(TriggerCategory.Condition, 105), IsVariableNotEqualToString,
                "and variable % does not equal {...},");

            // (1:106) and variable % is constant,
            Add(new Trigger(TriggerCategory.Condition, 106), VariableIsConstant,
                "and variable % is constant,");

            // (1:107) and variable % is not constant,
            Add(new Trigger(TriggerCategory.Condition, 107), VariableIsNotConstant,
                "and variable % is not constant,");

            // (5:100) set variable % to {...}.
            Add(new Trigger(TriggerCategory.Effect, 100), SetVariableToString,
                "set variable % to {...}.");

            // (5:101) set variable % to #.
            Add(new Trigger(TriggerCategory.Effect, 101), SetVariableToNumberOrVariable,
                "set variable % to #.");

            // (5:102) print {...} to the console.
            Add(new Trigger(TriggerCategory.Effect, 102), PrintToLog,
                "print {...} to the log.");

            // (5:103) get the environment variable named {...} and put it into #,
            Add(new Trigger(TriggerCategory.Effect, 103), GetEnvVariable,
                "get the environment variable named {...} and put it into %, (ex: PATH)");

            Add(TriggerCategory.Effect, 104, RandomValueToVar,
                "create random number and put it into variable %.");

            Add(new Trigger(TriggerCategory.Effect, 107), DeleteVariable,
                "delete variable %.");

            // (5:110) load library from file {...}.
            Add(new Trigger(TriggerCategory.Effect, 110), LoadLibraryFromFile,
                "load library from file {...}. (example Monkeyspeak.dll)");

            Add(new Trigger(TriggerCategory.Cause, 100), JobCalled,
                "when job # is called put arguments into table % (optional),");

            Add(new Trigger(TriggerCategory.Effect, 115), CallJob,
                "call job # with (add strings, variables, numbers here) arguments.");
        }

        private bool CallJob(TriggerReader reader)
        {
            double jobNumber = 0;
            if (reader.PeekVariable())
            {
                jobNumber = reader.ReadVariable().Value.As<double>();
            }
            else if (reader.PeekNumber())
            {
                jobNumber = reader.ReadNumber();
            }

            var args = reader.ReadValues().ToArray();
            if (jobNumber > 0)
                if (args == null || args.Length == 0)
                    reader.Page.Execute(100, jobNumber);
                else reader.Page.Execute(100, Enumerable.Concat(new object[] { jobNumber }, args).ToArray());
            return true;
        }

        private bool JobCalled(TriggerReader reader)
        {
            double jobNumber = 0;
            if (reader.PeekVariable())
            {
                jobNumber = reader.ReadVariable().Value.As<double>();
            }
            else if (reader.PeekNumber())
            {
                jobNumber = reader.ReadNumber();
            }

            double requiredJobNumber = reader.GetParameter<double>(0);

            if (reader.TryReadVariableTable(out VariableTable table, true))
            {
                object[] args = reader.Parameters.Skip(1).ToArray();
                for (int i = 0; i <= args.Length - 1; i++)
                    table.Add(i.ToString(), args[i]);
            }

            bool result = false;
            if (jobNumber > 0 && jobNumber == requiredJobNumber)
                result = reader.CurrentBlock.IndexOfTrigger(TriggerCategory.Effect, 115, reader.CurrentBlockIndex) == -1;
            return result;
        }

        private bool DeleteVariable(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            var.Value = null;
            return !var.IsConstant ? reader.Page.RemoveVariable(var.Name) : false;
        }

        private bool GetEnvVariable(TriggerReader reader)
        {
            string envVar = Environment.GetEnvironmentVariable(reader.ReadString());
            var var = reader.ReadVariable(true);
            var.Value = envVar;
            return true;
        }

        private bool IsVariableDefined(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            return var != null && reader.Page.HasVariable(var.Name);
        }

        private bool IsVariableNotDefined(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            return var == null || !reader.Page.HasVariable(var.Name);
        }

        private bool IsVariableEqualToNumberOrVar(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            double num = 0;
            return reader.ReadNumber() == var.Value.As<double>();
        }

        private bool IsVariableNotEqualToNumberOrVar(TriggerReader reader)
        {
            return !IsVariableEqualToNumberOrVar(reader);
        }

        private bool IsVariableEqualToString(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            if (var == null) return false;
            if (reader.PeekString())
            {
                var str = reader.ReadString();
                return var.Value.As<string>(string.Empty).Equals(str, StringComparison.InvariantCulture);
            }
            return false;
        }

        private bool IsVariableNotEqualToString(TriggerReader reader)
        {
            return !IsVariableEqualToString(reader);
        }

        private bool LoadLibraryFromFile(TriggerReader reader)
        {
            if (!reader.PeekString()) return false;
            reader.Page.LoadLibraryFromAssembly(reader.ReadString());
            return true;
        }

        private bool PrintToLog(TriggerReader reader)
        {
            string output = reader.ReadString();
            Logger.Info<Sys>(output);
            return true;
        }

        private bool RandomValueToVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = (double)new Random().Next();
            return true;
        }

        private bool SetVariableToNumberOrVariable(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            if (reader.PeekVariable<double>())
            {
                var.Value = reader.ReadVariable().Value.As(0d);
            }
            else if (reader.PeekNumber())
            {
                var.Value = reader.ReadNumber();
            }

            return true;
        }

        private bool SetVariableToString(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var str = reader.ReadString();
            var.Value = str;
            return true;
        }

        private bool VariableIsConstant(TriggerReader reader)
        {
            return reader.ReadVariable().IsConstant;
        }

        private bool VariableIsNotConstant(TriggerReader reader)
        {
            return !reader.ReadVariable().IsConstant;
        }

        public override void Unload(Page page)
        {
        }
    }
}