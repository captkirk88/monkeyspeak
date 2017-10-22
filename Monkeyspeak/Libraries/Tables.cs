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

        public override void Initialize()
        {
            Add(TriggerCategory.Flow, ForEntryInTable,
                "for each entry in table % put it into %,");

            Add(TriggerCategory.Effect, CreateTable,
                "create a table as %.");

            Add(TriggerCategory.Effect, PutNumIntoTable,
                "with table % put # in it at key {...}.");

            Add(TriggerCategory.Effect, PutStringIntoTable,
                "with table % put {...} in it at key {...}.");

            Add(TriggerCategory.Effect, GetTableKeyIntoVar,
                "with table % get key {...} put it in into variable %.");

            // (1:106) and variable % is constant,
            Add(TriggerCategory.Condition, VariableIsTable,
                "and variable % is a table,");

            // (1:107) and variable % is not constant,
            Add(TriggerCategory.Condition, VariableIsNotTable,
                "and variable % is not a table,");
        }

        private bool VariableIsNotTable(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            return !(var is VariableTable);
        }

        private bool VariableIsTable(TriggerReader reader)
        {
            var var = reader.ReadVariable();
            return var is VariableTable;
        }

        private bool ForEntryInTable(TriggerReader reader)
        {
            var table = reader.ReadVariableTable();
            var var = reader.ReadVariable(true);
            var.Value = table.Next();
            bool canContinue = true;
            if (table.CurrentElementIndex > table.Count) canContinue = false;
            if (!canContinue)
            {
                table.ResetIndex();
            }
            return canContinue;
        }

        private bool GetTableKeyIntoVar(TriggerReader reader)
        {
            var var = reader.ReadVariableTable();
            var key = reader.ReadString();
            var into = reader.ReadVariable(true);
            into.Value = var[key];
            return true;
        }

        private bool PutStringIntoTable(TriggerReader reader)
        {
            var var = reader.ReadVariableTable(true);
            var value = reader.ReadString();
            var key = reader.ReadString();
            var[key] = value;
            return true;
        }

        private bool PutNumIntoTable(TriggerReader reader)
        {
            var var = reader.ReadVariableTable(true);
            var value = reader.ReadVariableOrNumber();
            var key = reader.ReadString();
            var[key] = value;
            return true;
        }

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