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

        /// <summary> Returns the physical nametable (0 or 1 for H/V, 0-3 for FourScreen) for rendering. Index 0=$2000, 1=$2400, 2=$2800, 3=$2C00. </summary>
        public Nametable GetNametable(int index) {
            var i = index & 3;
            int bank;
            switch (Mirroring) {
                case Mirroring.SingleScreen:
                    bank = SingleScreenPage & 1;
                    break;
                case Mirroring.Vertical:
                    bank = i >> 1;
                    break;
                case Mirroring.Horizontal:
                    bank = i & 1;
                    break;
                default:
                    bank = i;
                    break;
            }
            return nametable[bank];
        }

        public Mirroring Mirroring { get; set; }

        public byte this[int address] {
            get {
                GetBankAndOffset(address, out int packed);
                return nametable[packed >> 10][packed & 0x3FF];
            }
            set {
                GetBankAndOffset(address, out int packed);
                nametable[packed >> 10][packed & 0x3FF] = value;
            }
        }

        /// <summary> Packs (physicalBank, offset) into a single int: bank in high bits, offset (0-0x3FF) in low. </summary>
        private void GetBankAndOffset(int address, out int packedBankAndOffset) {
            address &= 0xFFF;
            int bank;
            int offset;
            switch (Mirroring) {
                case Mirroring.FourScreen:
                    bank = address >> 10;
                    offset = address & 0x3FF;
                    break;
                case Mirroring.SingleScreen:
                    bank = SingleScreenPage & 1;
                    offset = address & 0x3FF;
                    break;
                case Mirroring.Vertical:
                    bank = address >= 0x800 ? 1 : 0;
                    offset = address & 0x3FF;
                    break;
                case Mirroring.Horizontal:
                    bank = (address >> 10) & 1;
                    offset = address & 0x3FF;
                    break;
                default:
                    bank = address >> 10;
                    offset = address & 0x3FF;
                    break;
            }
            packedBankAndOffset = (bank << 10) | offset;
        }

        public int Cells => nametable.Sum(n => n.Cells);

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