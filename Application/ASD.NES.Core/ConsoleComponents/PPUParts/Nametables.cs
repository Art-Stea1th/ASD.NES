using System.Linq;

namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    using BasicComponents;
    using Helpers;
    using Shared;

    internal sealed class Nametables : IMemory<Octet> {

        private readonly Nametable[] nametable = new Nametable[4].Initialize<Nametable>();

        public NametableMirroring Mirroring { get; set; }

        public Octet this[int address] {
            get => nametable[GetBankIndex(FixAddress(address))][address & 0x3FF];
            set => nametable[GetBankIndex(FixAddress(address))][address & 0x3FF] = value;
        }

        public int Cells => nametable.Sum(n => n.Cells);

        private int GetBankIndex(int fixedAddress) => fixedAddress >> 10; // n / 1024

        private int FixAddress(int address) {

            address &= 0xFFF;

            switch (Mirroring) {
                case NametableMirroring.FourScreen: break;
                case NametableMirroring.SingleScreen: address &= 0x3FF; break;
                case NametableMirroring.Vertical: address &= 0x7FF; break;
                case NametableMirroring.Horizontal:
                    address = address < 0x800
                        ? address & 0x3FF
                        : address & 0xBFF; break;
            }
            return address;
        }


        private sealed class Nametable : IMemory<Octet> {

            private readonly RefOctet[] symbols = new Octet[960].Wrap();
            private readonly RefOctet[] attributes = new Octet[64].Wrap();

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
        }
    }    
}