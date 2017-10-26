using System.Linq;

namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    using BasicComponents;
    using Helpers;
    using Shared;

    public enum Mirroring { FourScreen, SingleScreen, Vertical, Horizontal }

    internal sealed class Nametables : IMemory<Octet> {

        private readonly Nametable[] nametable = new Nametable[4].Initialize<Nametable>();

        public Nametable GetNametable(int index) => nametable[FixBankIndex(index)];

        public Mirroring Mirroring { get; set; }

        public Octet this[int address] {
            get => nametable[GetBankIndex(FixAddress(address))][address & 0x3FF];
            set => nametable[GetBankIndex(FixAddress(address))][address & 0x3FF] = value;
        }

        public int Cells => nametable.Sum(n => n.Cells);

        private int FixBankIndex(int index) => FixAddress((index & 0b11) << 10) >> 10;
        private int GetBankIndex(int fixedAddress) => fixedAddress >> 10; // n / 1024

        private int FixAddress(int address) { // redirect

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


        internal sealed class Nametable : IMemory<Octet> {

            private readonly RefOctet[] symbols = new Octet[960].Wrap();   // 32x30
            private readonly RefOctet[] attributes = new Octet[64].Wrap(); //  8x8

            public Octet this[int address] {
                get => Read(address);
                set => Write(address, value);
            }
            public int Cells => symbols.Length + attributes.Length;

            public Octet Read(int address) {

                address &= 0x3FF;

                if (address < 0x3C0) {
                    return symbols[address];
                }

                address &= 0x3F;
                return attributes[address];
            }

            public void Write(int address, Octet value) {

                address &= 0x3FF;

                if (address < 0x3C0) {
                    symbols[address].Value = value;
                }
                else {
                    address &= 0x3F;
                    attributes[address].Value = value;
                }
            }

            public Octet GetSymbol(int x, int y) => symbols[y << 5 | x];       // y * 32 + x
            public Octet GetAttribute(int x, int y) => attributes[y << 3 | x]; // y *  8 + x
        }
    }
}