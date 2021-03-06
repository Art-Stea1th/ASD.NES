﻿using System.Collections.Generic;
using System.Linq;

namespace ASD.NES.Core.Helpers {

    internal static class ExtensionsIEnumerable {

        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> sequence, int times)
            => Enumerable.Repeat(sequence, times).SelectMany(t => t);

        public static T[] Initialize<T>(this T[] array) where T : new() {
            for (var i = 0; i < array.Length; i++) {
                array[i] = new T();
            }
            return array;
        }
    }
}