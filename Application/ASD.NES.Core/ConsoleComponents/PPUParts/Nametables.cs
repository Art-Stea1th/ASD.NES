using System;
using System.Linq;

namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    using BasicComponents;
    using Helpers;

    public enum Mirroring { FourScreen, SingleScreen, Vertical, Horizontal }

    internal sealed class Nametables : IMemory<byte> {

        private readonly Nametable[] nametable = new Nametable[4].Initialize<Nametable>();

        internal void Clear() {
            for (var i = 0; i < nametable.Length; i++) {
                nametable[i].Clear();
            }
        }

        /// <summary> For SingleScreen: which 1KB page (0 or 1) is used for all 4 nametables. Used by AxROM bit 4. </summary>
        internal int SingleScreenPage { get; set; }

        public Nametable GetNametable(int index) => nametable[GetPhysicalBankIndex((index & 3) << 10)];

        public Mirroring Mirroring { get; set; }

        public byte this[int address] {
            get => nametable[GetPhysicalBankIndex(FixAddress(address))][address & 0x3FF];
            set => nametable[GetPhysicalBankIndex(FixAddress(address))][address & 0x3FF] = value;
        }

        private int GetPhysicalBankIndex(int fixedAddress) {
            if (Mirroring == Mirroring.SingleScreen)
                return SingleScreenPage & 1;
            return fixedAddress >> 10;
        }

        public int Cells => nametable.Sum(n => n.Cells);

        private int FixAddress(int address) { // redirect // TODO: Validate, handle another mirroring modes

            address &= 0xFFF;

            switch (Mirroring) {
                case Mirroring.FourScreen: break;
                case Mirroring.SingleScreen: address &= 0x3FF; break;
                case Mirroring.Vertical: address &= 0x7FF; break;
                case Mirroring.Horizontal:
                    address = address < 0x800
                        ? address & 0x3FF
                        : address & 0xBFF; break;
            }
            return address;
        }

        internal sealed class Nametable : IMemory<byte> {

            private readonly byte[] symbols = new byte[960];   // 32x30
            private readonly byte[] attributes = new byte[64]; //  8x8

            public byte this[int address] {
                get => Read(address);
                set => Write(address, value);
            }
            public int Cells => symbols.Length + attributes.Length;

            internal void Clear() {
                Array.Clear(symbols, 0, symbols.Length);
                Array.Clear(attributes, 0, attributes.Length);
            }

            public byte Read(int address) {

                address &= 0x3FF;

                if (address < 0x3C0) {
                    return symbols[address];
                }

                address &= 0x3F;
                return attributes[address];
            }

            public void Write(int address, byte value) {

                address &= 0x3FF;

                if (address < 0x3C0) {
                    symbols[address] = value;
                }
                else {
                    address &= 0x3F;
                    attributes[address] = value;
                }
            }

            public byte GetSymbol(int x, int y) => symbols[y << 5 | x];       // y * 32 + x
            public byte GetAttribute(int x, int y) => attributes[y << 3 | x]; // y *  8 + x
        }
    }
}