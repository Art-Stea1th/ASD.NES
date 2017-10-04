using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ASD.NESCore.Common {

    using Helpers;

    internal class MemoryBank : IReadOnlyCollection<RInt8> {

        private RInt8[] data;
        public int Count => data.Length;
        public RInt8 this[int address] => data[address];

        public MemoryBank(int bytes, int multiplier = 1) {
            if (bytes < 1 && multiplier < 1) {
                throw MustBeGreaterThanZero(nameof(bytes), nameof(multiplier));
            }
            data = new RInt8[bytes].Repeat(multiplier).ToArray();
        }

        public MemoryBank(IEnumerable<byte> bytes, int multiplier = 1) {
            if (bytes.Count() < 1 || multiplier < 1) {
                throw MustBeGreaterThanZero(nameof(bytes), nameof(multiplier));
            }
            data = bytes.Select(b => RInt8.Wrap(b)).Repeat(multiplier).ToArray();
        }

        private ArgumentException MustBeGreaterThanZero(string nameA, string nameB)
            => new ArgumentException($"The '{nameA}' count & '{nameB}' count must be greater than 0.");

        public IEnumerator<RInt8> GetEnumerator() => data.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => data.GetEnumerator();
    }
}