using System;

namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    using BasicComponents;
    using CartridgeComponents.Boards;

    internal sealed class PPUAddressSpace : IMemory<byte> {

        #region Singleton
        public static PPUAddressSpace Instance => instance.Value;

        private static readonly Lazy<PPUAddressSpace> instance = new Lazy<PPUAddressSpace>(() => new PPUAddressSpace());
        private PPUAddressSpace() { }
        #endregion

        private static Board externalMemory;           // $0000 - $1FFF: CHR-ROM Tite-set 0 and 1 - 8 kb (2x4 kb)

        private static readonly Nametables nametables; // $2000 - $2FFF: Nametables - 4 kb + $3000 - 3EFF: mirror
        private static readonly byte[] palettes;       // $3F00 - $3FFF: Palettes (BG - 16 b \ Sprite - 16 b) - Mirror x 8 (256 b)

        public Nametables.Nametable GetNametable(int index) => nametables.GetNametable(index);

        public Mirroring NametableMirroring {
            get => nametables.Mirroring;
            set => nametables.Mirroring = value;
        }

        /// <summary> For SingleScreen mirroring: which 1KB page (0 or 1) is used. AxROM sets this via register bit 4. </summary>
        internal int SingleScreenPage {
            get => nametables.SingleScreenPage;
            set => nametables.SingleScreenPage = value;
        }

        public byte this[int address] {
            get => Read(address);
            set => Write(address, value);
        }
        public int Cells => 1024 * 16;
        public int LastAddress => Cells - 1;

        static PPUAddressSpace() {
            nametables = new Nametables();
            palettes = new byte[32];
        }

        public void SetExternalMemory(Board boardMemory) {
            externalMemory = boardMemory;
        }

        /// <summary> Clears nametables and palettes (e.g. when swapping cartridge so the new game starts with a clean PPU). </summary>
        internal void ClearVideoState() {
            nametables.Clear();
            Array.Clear(palettes, 0, palettes.Length);
        }

        private byte Read(int address) {

            if (address < 0x2000) {
                return externalMemory[address | 0x6000];
            }
            if (address < 0x3F00) {
                return nametables[address];
            }
            var i = address & 0x1F;
            if (i == 4 || i == 8 || i == 12) return palettes[0];
            if (i == 20 || i == 24 || i == 28) return palettes[16];
            return palettes[i];
        }

        private void Write(int address, byte value) {

            if (address < 0x2000) {
                externalMemory[address | 0x6000] = value;
            }
            else if (address < 0x3F00) {
                nametables[address] = value;
            }
            else {
                var i = address & 0x1F;
                palettes[i] = value;
                // NES quirk: $3F04/$3F08/$3F0C mirror $3F00 (backdrop); $3F14/$3F18/$3F1C mirror $3F10
                if (i == 4 || i == 8 || i == 12) palettes[0] = value;
                if (i == 20 || i == 24 || i == 28) palettes[16] = value;
            }
        }
    }
}