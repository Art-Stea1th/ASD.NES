using System;

namespace ASD.NES.Core.ConsoleComponents.APUParts.Registers {

    using BasicComponents;

    internal class AudioChannelRegisters : IMemory<byte> {

        private byte[] r = new byte[4];
        public byte this[int address] {
            get => r[address & 0b11];
            set {
                r[address &= 0b11] = value;
                Changed?.Invoke(address);
            }
        }
        public int Cells => r.Length;
        public Action<int> Changed { get; set; }
    }
}