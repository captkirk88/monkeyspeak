using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Extensions
{
    public static class VariablesExtension
    {
        public static VariableTable ConvertToTable(this IVariable var, Page page)
        {
            if (var is VariableTable tableVar) return tableVar;
            page.RemoveVariable(var);
            var table = page.CreateVariableTable(var.Name, var.IsConstant);
            if (var.Value != null)
                table.Add("0", var.Value);
            return table;
        }

        public static ObjectVariable ConvertToObjectVariable(this IVariable var, Page page)
        {
            if (var is ObjectVariable objVar) return objVar;
            page.RemoveVariable(var);
            var obj = page.SetVariable(new ObjectVariable(var.Name));
            if (var.Value != null)
                obj.Value = var.Value;
            return obj;
        }

        public static ObjectVariable ConvertToObjectVariable(this VariableTable table, Page page)
        {
            page.RemoveVariable(table.Name);
            var obj = page.SetVariable(new ObjectVariable(table.Name));
            if (table.Count > 0)
            {
                return page.SetVariable(new ObjectVariable(table.Name, table.values));
            }
            return page.SetVariable(new ObjectVariable(table.Name, table.Value));
        }
    }
}