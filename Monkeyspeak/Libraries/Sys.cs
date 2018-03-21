using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Monkeyspeak.Libraries
{
    /// <summary>
    /// </summary>
    /// <seealso cref="BaseLibrary"/>
    public class Sys : BaseLibrary
    {
        /// <summary>
        /// Initializes this instance. Add your trigger handlers here.
        /// </summary>
        /// <param name="args">
        /// Parametized argument of objects to use to pass runtime objects to a library at initialization
        /// </param>
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

        [TriggerDescription("Calls the job")]
        [TriggerNumberParameter]
        [TriggerValuesParameter("Parameters to call the job with")]
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

        [TriggerDescription("Occurs when a job number is called")]
        [TriggerNumberParameter]
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

        [TriggerDescription("Deletes the variable, this is not recoverable")]
        [TriggerVariableParameter("The variable")]
        private bool DeleteVariable(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            var.Value = null;
            return !var.IsConstant ? reader.Page.RemoveVariable(var.Name) : false;
        }

        [TriggerDescription("Gets the environment variable and puts it into a variable")]
        [TriggerStringParameter]
        [TriggerVariableParameter]
        private bool GetEnvVariable(TriggerReader reader)
        {
            string envVar = Environment.GetEnvironmentVariable(reader.ReadString());
            var var = reader.ReadVariable(true);
            var.Value = envVar;
            return true;
        }

        [TriggerDescription("Determines whether the variable is defined or not")]
        private bool IsVariableDefined(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            return var != null && reader.Page.HasVariable(var.Name);
        }

        [TriggerDescription("Determines whether the variable is not defined")]
        [TriggerVariableParameter]
        private bool IsVariableNotDefined(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            return var == null || !reader.Page.HasVariable(var.Name);
        }

        [TriggerDescription("Determines whether the variable is equal to a number or variable that contains a number value")]
        [TriggerVariableParameter]
        [TriggerNumberParameter]
        private bool IsVariableEqualToNumberOrVar(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            return reader.ReadNumber() == var.Value.AsDouble();
        }

        [TriggerDescription("Determines whether the variable is not equal to a number or variable that contains a number value")]
        [TriggerVariableParameter]
        [TriggerNumberParameter]
        private bool IsVariableNotEqualToNumberOrVar(TriggerReader reader)
        {
            return !IsVariableEqualToNumberOrVar(reader);
        }

        [TriggerDescription("Determines whether the variable is equal to a string")]
        [TriggerVariableParameter]
        [TriggerStringParameter]
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

        [TriggerDescription("Determines whether the variable is not equal to a string")]
        [TriggerVariableParameter]
        [TriggerStringParameter]
        private bool IsVariableNotEqualToString(TriggerReader reader)
        {
            return !IsVariableEqualToString(reader);
        }

        [TriggerDescription("Gets all the loaded libraries and puts them into a table")]
        [TriggerVariableParameter]
        private bool GetAllLoadedLibrariesIntoTable(TriggerReader reader)
        {
            var table = reader.ReadVariableTable(true);
            foreach (var lib in reader.Page.Libraries)
            {
                table.Add(lib.GetType().Name);
            }
            return true;
        }

        [TriggerDescription("Unloads the library")]
        [TriggerStringParameter]
        private bool UnloadLibrary(TriggerReader reader)
        {
            var libName = reader.ReadString();
            var lib = reader.Page.Libraries.FirstOrDefault(l => l.GetType().Name == libName);
            reader.Page.RemoveLibrary(lib);
            return true;
        }

        [TriggerDescription("Loads the library from the specified file")]
        [TriggerStringParameter]
        private bool LoadLibraryFromFile(TriggerReader reader)
        {
            if (!reader.PeekString()) return false;
            reader.Page.LoadLibraryFromAssembly(reader.ReadString());
            return true;
        }

        /// <summary>
        /// Prints to log.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        [TriggerDescription("Prints to the log")]
        [TriggerStringParameter]
        public virtual bool PrintToLog(TriggerReader reader)
        {
            string output = reader.ReadString();
            Logger.Info<Sys>(output);
            //Console.WriteLine(output);
            return true;
        }

        [TriggerDescription("Puts a random number into a variable")]
        [TriggerNumberParameter]
        [TriggerVariableParameter]
        private bool RandomValueToVar(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var.Value = (double)new Random().Next();
            return true;
        }

        [TriggerDescription("Sets a variable to a number or variable as double")]
        [TriggerVariableParameter]
        [TriggerNumberParameter]
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

        [TriggerDescription("Sets a variable to a string")]
        [TriggerVariableParameter]
        [TriggerStringParameter]
        private bool SetVariableToString(TriggerReader reader)
        {
            var var = reader.ReadVariable(true);
            var str = reader.ReadString();
            var.Value = str;
            return true;
        }

        [TriggerDescription("Determines whether the variable is constant/unmodifiable")]
        [TriggerVariableParameter]
        private bool VariableIsConstant(TriggerReader reader)
        {
            return reader.ReadVariable().IsConstant;
        }

        [TriggerDescription("Determines whether the variable is not constant/unmodifiable")]
        [TriggerVariableParameter]
        private bool VariableIsNotConstant(TriggerReader reader)
        {
            return !reader.ReadVariable().IsConstant;
        }

        /// <summary>
        /// @ Called when page is disposing or resetting.
        /// </summary>
        /// <param name="page">The page.</param>
        public override void Unload(Page page)
        {
        }
    }
}