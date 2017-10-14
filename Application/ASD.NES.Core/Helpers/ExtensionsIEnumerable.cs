using System.Collections.Generic;
using System.Linq;

namespace ASD.NES.Core.Helpers {

    using Shared;

    internal static class ExtensionsIEnumerable {

        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> sequence, int times)
            => Enumerable.Repeat(sequence, times).SelectMany(t => t);

        public static RefOctet[] Convert(this byte[] array)
            => array.Select(b => RefOctet.Wrap(b)).ToArray();
    }
}