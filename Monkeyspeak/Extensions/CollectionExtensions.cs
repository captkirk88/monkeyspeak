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
        /// <returns><c>true</c> if item was added, <c>false</c> if it was not</returns>
        public static bool AddIfUnique<T>(this ICollection<T> col, T item)
        {
            if (!col.Contains(item))
            {
                col.Add(item);
                return true;
            }
            return false;
        }
    }
}