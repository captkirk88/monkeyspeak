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

            Add(TriggerCategory.Effect, PutNumIntoTable,
                "with table % put # in it at key {...}.");

            Add(TriggerCategory.Effect, PutStringIntoTable,
                "with table % put {...} in it at key {...}.");

            Add(TriggerCategory.Effect, GetTableKeyIntoVar,
                "with table % get key {...} put it in into variable %.");

            Add(TriggerCategory.Effect, ClearTable,
                "with table % remove all entries in it.");

            Add(TriggerCategory.Condition, VariableIsTable,
                "and variable % is a table,");

            Add(TriggerCategory.Condition, VariableIsNotTable,
                "and variable % is not a table,");
        }

        [TriggerDescription("Clears the table")]
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

        [TriggerDescription("Gets the key in a table and puts it into a variable")]
        [TriggerStringParameter]
        [TriggerVariableParameter]
        private bool GetTableKeyIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariableTable();
            var key = reader.ReadString();
            var into = reader.ReadVariable(true);
            into.Value = var[key];
            return true;
        }

        [TriggerDescription("Puts the string into a table at the specified key, if the key doesn't exist, it will be created")]
        [TriggerVariableParameter]
        [TriggerStringParameter]
        [TriggerStringParameter]
        private bool PutStringIntoTable(TriggerReader reader)
        {
            var var = reader.ReadVariableTable(true);
            var value = reader.ReadString();
            var key = reader.ReadString();
            var.Add(key, value);
            return true;
        }

        [TriggerDescription("Puts the number into a table at the specified key, if the key doesn't exist, it will be created")]
        [TriggerVariableParameter]
        [TriggerNumberParameter]
        [TriggerStringParameter]
        private bool PutNumIntoTable(TriggerReader reader)
        {
            var var = reader.ReadVariableTable(true);
            var value = reader.ReadNumber();
            var key = reader.ReadString();
            var.Add(key, value);
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