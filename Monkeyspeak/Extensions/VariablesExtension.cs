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
            page.RemoveVariable(var);
            var table = page.SetVariableTable(var.Name, var.IsConstant);
            table.Add("value", var.Value);
            return table;
        }
    }
}