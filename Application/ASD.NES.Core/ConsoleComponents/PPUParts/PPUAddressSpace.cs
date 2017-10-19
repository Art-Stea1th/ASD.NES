using System;
using System.Linq;

namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    using BasicComponents;
    using Helpers;
    using Shared;

    public enum NametableMirroring { FourScreen, SingleScreen, Vertical, Horizontal }

    internal sealed class PPUAddressSpace : IMemory<Octet> {

        #region Singleton
        public static PPUAddressSpace Instance => instance.Value;

        private static readonly Lazy<PPUAddressSpace> instance = new Lazy<PPUAddressSpace>(() => new PPUAddressSpace());
        private PPUAddressSpace() { }
        #endregion

        private static readonly Octet[] tileSet0;     // $0000 - $0FFF: CHR-ROM Tite-set 0 - 4 kb
        private static readonly Octet[] tileSet1;     // $1000 - $1FFF: CHR-ROM Tite-set 1 - 4 kb

        public static readonly Nametables Nametables; // $2000 - $2FFF: Nametables - 4 kb + $3000 - 3EFF: mirror
        private static readonly RefOctet[] palettes;  // $3F00 - $3FFF: Palettes (BG - 16 b \ Sprite - 16 b) - Mirror x 8 (256 b)

        public NametableMirroring NametableMirroring {
            get => Nametables.Mirroring;
            set => Nametables.Mirroring = value;
        }

        public Octet this[int address] {
            get => Read(address);
            set => Write(address, value);
        }
        public int Cells => 1024 * 16;
        public int LastAddress => Cells - 1;

        static PPUAddressSpace() {
            tileSet0 = new Octet[4096];
            tileSet1 = new Octet[4096];
            Nametables = new Nametables();
            palettes = new Octet[32].Wrap().Repeat(8).ToArray();
        }

        private Octet Read(int address) {

            if (address < 0x1000) {
                return tileSet0[address & 0xFFF];
            }
            if (address < 0x2000) {
                return tileSet1[address & 0xFFF];
            }
            if (address < 0x3EFF) {
                return Nametables[address];
            }
            return palettes[address & 0xFF];
        }

        private void Write(int address, Octet value) {

            if (address < 0x1000) {
                tileSet0[address & 0xFFF] = value;
            }
            else if (address < 0x2000) {
                tileSet1[address & 0xFFF] = value;
            }
            else if (address < 0x3EFF) {
                Nametables[address] = value;
            }
            else {
                palettes[address & 0xFF].Value = value;
            }
        }
    }
}