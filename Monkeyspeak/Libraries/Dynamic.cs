using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monkeyspeak.Extensions;

namespace Monkeyspeak.Libraries
{
    public sealed class Dynamic : AutoIncrementBaseLibrary
    {
        public override int BaseId => 270;

        public override void Initialize(params object[] args)
        {
            Add(TriggerCategory.Effect, CreateObjVar,
                "create a object variable %");

            Add(TriggerCategory.Effect, ConvertTableToObjVar,
                "take table % and make it a object variable");
        }

        private bool ConvertTableToObjVar(TriggerReader reader)
        {
            var table = reader.ReadVariableTable();
            if (table != VariableTable.Empty)
            {
                table.ConvertToObjectVariable(reader.Page);
                return true;
            }
            return false;
        }

        private bool CreateObjVar(TriggerReader reader)
        {
            var obj = reader.ReadObjectVariable(true);
            return true;
        }

        public override void Unload(Page page)
        {
        }
    }
}