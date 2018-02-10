using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Libraries
{
    public class Tables : AutoIncrementBaseLibrary
    {
        public override int BaseId => 250;

        public override void Initialize(params object[] args)
        {
            Add(TriggerCategory.Flow, ForEntryInTable,
                "for each entry in table % put it into %,");

            Add(TriggerCategory.Flow, ForKeyValueInTable,
                "for each key/value pair in table % put them into % and %,");

            Add(TriggerCategory.Effect, CreateTable,
                "create a table as %.");

            Add(TriggerCategory.Effect, ClearTable,
                "with table % remove all entries in it.");

            Add(TriggerCategory.Effect, ClearTableEntry,
                "with table % remove key {...}");

            Add(TriggerCategory.Condition, VariableIsTable,
                "and variable % is a table,");

            Add(TriggerCategory.Condition, VariableIsNotTable,
                "and variable % is not a table,");

            Add(TriggerCategory.Condition, TableContains,
                "and table % contains {...}");

            Add(TriggerCategory.Condition, TableNotContains,
                "and table % does not contain {...}");

            Add(TriggerCategory.Condition, TableContainsNumber,
                "and table % contains #");

            Add(TriggerCategory.Condition, TableNotContainsNumber,
                "and table % does not contain #");

            Add(TriggerCategory.Effect, MergeIntoVariable,
                "with table % join the contents and put it into variable %");
        }

        [TriggerDescription("Merges the table contents into a string and puts the result into a variable")]
        [TriggerStringParameter]
        [TriggerStringParameter]
        [TriggerVariableParameter]
        private bool MergeIntoVariable(TriggerReader reader)
        {
            var table = reader.ReadVariableTable();
            var var = reader.ReadVariable(true);

            var mergedStr = string.Join(" ", table.Values);
            var.Value = mergedStr;
            return true;
        }

        [TriggerDescription("Determines if the table does not contain the number")]
        [TriggerVariableParameter]
        [TriggerStringParameter]
        private bool TableNotContainsNumber(TriggerReader reader)
        {
            var var = reader.ReadVariableTable();
            var key = reader.ReadNumber();
            foreach (var entry in var)
            {
                if (entry.Value.AsDouble() == key)
                    return false;
            }
            return true;
        }

        [TriggerDescription("Determines if the table does not contain the number")]
        [TriggerVariableParameter]
        [TriggerStringParameter]
        private bool TableContainsNumber(TriggerReader reader)
        {
            var var = reader.ReadVariableTable();
            var key = reader.ReadNumber();
            foreach (var entry in var)
            {
                if (entry.Value.AsDouble() == key)
                    return true;
            }
            return false;
        }

        [TriggerDescription("Determines if the table does not contain the string")]
        [TriggerVariableParameter]
        [TriggerStringParameter]
        private bool TableNotContains(TriggerReader reader)
        {
            var var = reader.ReadVariableTable();
            var key = reader.ReadString();
            foreach (var entry in var)
            {
                if (entry.Value.AsString() == key)
                    return false;
            }
            return true;
        }

        [TriggerDescription("Determines if the table contains the string")]
        [TriggerVariableParameter]
        [TriggerStringParameter]
        private bool TableContains(TriggerReader reader)
        {
            var var = reader.ReadVariableTable();
            var key = reader.ReadString();
            foreach (var entry in var)
            {
                if (entry.Value.AsString() == key)
                    return true;
            }
            return false;
        }

        [TriggerDescription("Removes the key from the table")]
        [TriggerVariableParameter]
        [TriggerStringParameter]
        private bool ClearTableEntry(TriggerReader reader)
        {
            var var = reader.ReadVariableTable();
            var key = reader.ReadString();
            var.Remove(key);
            return true;
        }

        [TriggerDescription("Removes everything from the table")]
        [TriggerVariableParameter]
        private bool ClearTable(TriggerReader reader)
        {
            var var = reader.ReadVariableTable();
            var.Clear();
            return true;
        }

        [TriggerDescription("Checks to see if variable is not a table")]
        [TriggerVariableParameter]
        private bool VariableIsNotTable(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            return !(var is VariableTable);
        }

        [TriggerDescription("Checks to see if variable is a table")]
        [TriggerVariableParameter]
        private bool VariableIsTable(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            return var is VariableTable;
        }

        [TriggerDescription("Iterates through a table")]
        [TriggerParameter("Variable to assign the table key on each iteration")]
        [TriggerParameter("Variable to assign the table value on each iteration")]
        private bool ForKeyValueInTable(TriggerReader reader)
        {
            var table = reader.ReadVariableTable();
            var key = reader.ReadVariable(true);
            var val = reader.ReadVariable(true);
            if (!table.Next(out object keyVal))
            {
                reader.Page.RemoveVariable(key);
                reader.Page.RemoveVariable(val);
                return false;
            }
            key.Value = table.ActiveIndexer;
            val.Value = keyVal;
            return true;
        }

        [TriggerDescription("Iterates through a table")]
        [TriggerParameter("Variable to assign the table entry on each iteration")]
        private bool ForEntryInTable(TriggerReader reader)
        {
            var table = reader.ReadVariableTable();
            var var = reader.ReadVariable(true);
            if (!table.Next(out object keyVal))
            {
                reader.Page.RemoveVariable(var);
                return false;
            }
            var.Value = keyVal;
            return true;
        }

        [TriggerDescription("Creates a table or clears a table if the specified table already exists")]
        [TriggerVariableParameter]
        private bool CreateTable(TriggerReader reader)
        {
            reader.ReadVariableTable(true);
            return true;
        }

        public override void Unload(Page page)
        {
        }
    }
}