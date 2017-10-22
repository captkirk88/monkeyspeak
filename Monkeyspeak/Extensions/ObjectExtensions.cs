using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Extensions
{
    public static class ObjectExtensions
    {
        public static T As<T>(this object obj, T @default = default(T))
        {
            if (obj == null) return @default;
            try
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
            catch { return @default; }
        }
    }
}