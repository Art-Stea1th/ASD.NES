using System.Collections.Generic;
using System.Linq;

namespace ASD.NESCore.Helpers {

    internal static class ExtensionsIEnumerable {

        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> sequence, int times)
            => Enumerable.Repeat(sequence, times).SelectMany(t => t);

    }
}