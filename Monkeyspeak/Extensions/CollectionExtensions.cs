using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddIfUnique<T>(this ICollection<T> col, T item)
        {
            if (!col.Contains(item)) col.Add(item);
        }
    }
}