using System;
using System.Linq;

namespace ASD.NESCore.Common {

    internal sealed class MemoryBus {

        private static Lazy<MemoryBus> instance = new Lazy<MemoryBus>(() => new MemoryBus());

        private static readonly RInt8[] memory;

        public byte this[ushort address] {
            get => memory[address];
            set => memory[address].Value = value;
        }

        public RInt8 GetReference(int address)
            => memory[address];

        public RInt8[] GetReferenceRange(int startAddress, int count)
            => memory.Skip(startAddress).Take(count).ToArray();

        public static MemoryBus Instance => instance.Value;

        private MemoryBus() { }

    }
}