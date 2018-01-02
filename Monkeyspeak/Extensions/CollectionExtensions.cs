using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds to the collection if the item is not in the collection already.
        /// </summary>
        /// <typeparam name="T">The collection's element type</typeparam>
        /// <param name="col">The collection.</param>
        /// <param name="item">The item.</param>
        public static void AddIfUnique<T>(this ICollection<T> col, T item)
        {
            if (!col.Contains(item)) col.Add(item);
        }
    }
}