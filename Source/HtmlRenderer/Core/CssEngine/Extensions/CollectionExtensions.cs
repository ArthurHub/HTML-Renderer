using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class CollectionExtensions
    {
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> items, T element)
        {
            foreach (var item in items) yield return item;

            yield return element;
        }

        public static T GetItemByIndex<T>(this IEnumerable<T> items, int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

            var i = 0;

            foreach (var item in items)
            {
                if (i++ == index)
                {
                    return item;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public static IEnumerable<object[]> ToObjectArray<T>(this IEnumerable<T> items)
            => items.Select(i => new object[] { i });
    }
}