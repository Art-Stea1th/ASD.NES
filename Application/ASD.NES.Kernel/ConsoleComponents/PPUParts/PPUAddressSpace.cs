using System;
using System.Linq;

namespace ASD.NES.Kernel.ConsoleComponents.PPUParts {

    using BasicComponents;
    using CartridgeComponents.Boards;
    using Helpers;
    using Shared;    

    internal sealed class PPUAddressSpace : IMemory<Octet> {

        #region Singleton
        public static PPUAddressSpace Instance => instance.Value;

        private static readonly Lazy<PPUAddressSpace> instance = new Lazy<PPUAddressSpace>(() => new PPUAddressSpace());
        private PPUAddressSpace() { }
        #endregion

        private static Board externalMemory;           // $0000 - $1FFF: CHR-ROM Tite-set 0 and 1 - 8 kb (2x4 kb)

        private static readonly Nametables nametables; // $2000 - $2FFF: Nametables - 4 kb + $3000 - 3EFF: mirror
        private static readonly RefOctet[] palettes;   // $3F00 - $3FFF: Palettes (BG - 16 b \ Sprite - 16 b) - Mirror x 8 (256 b)

        public Nametables.Nametable GetNametable(int index) => nametables.GetNametable(index);

        public Mirroring NametableMirroring {
            get => nametables.Mirroring;
            set => nametables.Mirroring = value;
        }

        public Octet this[int address] {
            get => Read(address);
            set => Write(address, value);
        }
        public int Cells => 1024 * 16;
        public int LastAddress => Cells - 1;

        static PPUAddressSpace() {
            nametables = new Nametables();
            palettes = new Octet[32].Wrap().Repeat(8).ToArray();
        }

        public void SetExternalMemory(Board boardMemory) {
            externalMemory = boardMemory;
        }

        private Octet Read(int address) {

            if (address < 0x2000) {
                return externalMemory[address | 0x6000];
            }
            if (address < 0x3EFF) {
                return nametables[address];
            }
            return palettes[address & 0xFF];
        }

        private void Write(int address, Octet value) {

            if (address < 0x2000) {
                externalMemory[address | 0x6000] = value;
            }
            else if (address < 0x3EFF) {
                nametables[address] = value;
            }
            else {
                palettes[address & 0xFF].Value = value;
            }
        }
    }
}