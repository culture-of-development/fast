using System;
using System.Collections.Generic;
using System.Linq;

namespace fast.helpers
{
    public static class CollectionHelpers
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
    }
}