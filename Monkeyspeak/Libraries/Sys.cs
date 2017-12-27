using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Monkeyspeak.Libraries
{
    public class Sys : BaseLibrary
    {
        public override void Initialize(params object[] args)
        {
            // (1:100) and variable % is defined,
            Add(TriggerCategory.Condition, 100, IsVariableDefined,
                "and variable % is defined,");

            // (1:101) and variable % is not defined,
            Add(TriggerCategory.Condition, 101, IsVariableNotDefined,
                "and variable % is not defined,");

            // (1:102) and variable % equals #,
            Add(TriggerCategory.Condition, 102, IsVariableEqualToNumberOrVar,
                "and variable % equals #,");

            // (1:103) and variable % does not equal #,
            Add(TriggerCategory.Condition, 103, IsVariableNotEqualToNumberOrVar,
                "and variable % does not equal #,");

            // (1:104) and variable % equals {...},
            Add(TriggerCategory.Condition, 104, IsVariableEqualToString,
                "and variable % equals {...},");

            // (1:105) and variable % does not equal {...},
            Add(TriggerCategory.Condition, 105, IsVariableNotEqualToString,
                "and variable % does not equal {...},");

            // (1:106) and variable % is constant,
            Add(TriggerCategory.Condition, 106, VariableIsConstant,
                "and variable % is constant,");

            // (1:107) and variable % is not constant,
            Add(TriggerCategory.Condition, 107, VariableIsNotConstant,
                "and variable % is not constant,");

            // (5:100) set variable % to {...}.
            Add(TriggerCategory.Effect, 100, SetVariableToString,
                "set variable % to {...}.");

            // (5:101) set variable % to #.
            Add(TriggerCategory.Effect, 101, SetVariableToNumberOrVariable,
                "set variable % to #.");

            // (5:102) print {...} to the console.
            Add(TriggerCategory.Effect, 102, PrintToLog,
                "print {...} to the log.");

            Add(TriggerCategory.Effect, 103, GetEnvVariable,
                "get the environment variable named {...} and put it into %, (ex: PATH)");

            Add(TriggerCategory.Effect, 104, RandomValueToVar,
                "create random number and put it into variable %.");

            Add(TriggerCategory.Effect, 107, DeleteVariable,
                "delete variable %.");

            Add(TriggerCategory.Effect, 110, LoadLibraryFromFile,
                "load library from file {...}. (example Monkeyspeak.dll)");

            Add(TriggerCategory.Effect, 111, UnloadLibrary,
                "unload library {...}. (example Sys)");

            Add(TriggerCategory.Effect, 112, GetAllLoadedLibrariesIntoTable,
                "get all loaded libraries and put them into table %.");

            Add(TriggerCategory.Cause, 100, JobCalled,
                "when job # is called put arguments into table % (optional),");

            Add(TriggerCategory.Effect, 115, CallJob,
                "call job # with (add strings, variables, numbers here) arguments.");
        }

        private bool CallJob(TriggerReader reader)
        {
            double jobNumber = 0;
            if (reader.PeekVariable())
            {
                jobNumber = reader.ReadVariable().Value.AsDouble();
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
                jobNumber = reader.ReadVariable().Value.AsDouble();
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
            return reader.ReadNumber() == var.Value.AsDouble();
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
                return var.Value.AsString(string.Empty).Equals(str, StringComparison.InvariantCulture);
            }
            return false;
        }

        private bool IsVariableNotEqualToString(TriggerReader reader)
        {
            return !IsVariableEqualToString(reader);
        }

        private bool GetAllLoadedLibrariesIntoTable(TriggerReader reader)
        {
            var table = reader.ReadVariableTable(true);
            foreach (var lib in reader.Page.Libraries)
            {
                table.Add(lib.GetType().Name);
            }
            return true;
        }

        private bool UnloadLibrary(TriggerReader reader)
        {
            var libName = reader.ReadString();
            var lib = reader.Page.Libraries.FirstOrDefault(l => l.GetType().Name == libName);
            reader.Page.RemoveLibrary(lib);
            return true;
        }

        private bool LoadLibraryFromFile(TriggerReader reader)
        {
            if (!reader.PeekString()) return false;
            reader.Page.LoadLibraryFromAssembly(reader.ReadString());
            return true;
        }

        public virtual bool PrintToLog(TriggerReader reader)
        {
            string output = reader.ReadString();
            Logger.Info<Sys>(output);
            //Console.WriteLine(output);
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
                var.Value = reader.ReadVariable().Value.AsDouble(0d);
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