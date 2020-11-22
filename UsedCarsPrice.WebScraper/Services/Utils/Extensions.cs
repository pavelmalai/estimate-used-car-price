using System;
using System.Collections.Generic;
using System.Linq;

namespace UsedCarsPrice.WebScraper.Services.Utils
{
    static class LinqExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            return splits;
        }

        public static IEnumerable<List<T>> SplitListByChunkSize<T>(this List<T> list, int nSize = 1000)
        {
            for (int i = 0; i < list.Count; i += nSize)
            {
                yield return list.GetRange(i, Math.Min(nSize, list.Count - i));
            }
        }
    }
}
