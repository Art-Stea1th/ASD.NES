using System.Linq;

namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    using BasicComponents;
    using Helpers;

    public enum Mirroring { FourScreen, SingleScreen, Vertical, Horizontal }

    internal sealed class Nametables : IMemory<byte> {

        private readonly Nametable[] nametable = new Nametable[4].Initialize<Nametable>();

        public Nametable GetNametable(int index) => nametable[FixBankIndex(index)];

        public Mirroring Mirroring { get; set; }

        public byte this[int address] {
            get => nametable[GetBankIndex(FixAddress(address))][address & 0x3FF];
            set => nametable[GetBankIndex(FixAddress(address))][address & 0x3FF] = value;
        }

        public int Cells => nametable.Sum(n => n.Cells);

        private int FixBankIndex(int index) => FixAddress((index & 0b11) << 10) >> 10;
        private int GetBankIndex(int fixedAddress) => fixedAddress >> 10; // n / 1024

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