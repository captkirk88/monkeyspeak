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
            if (var is VariableTable) return (VariableTable)var;
            page.RemoveVariable(var);
            var table = page.CreateVariableTable(var.Name, var.IsConstant);
            if (var.Value != null)
                table.Add("0", var.Value);
            return table;
        }
    }
}