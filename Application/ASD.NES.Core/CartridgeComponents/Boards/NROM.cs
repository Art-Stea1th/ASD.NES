using System;


namespace ASD.NES.Core.CartridgeComponents.Boards {

    // https://wiki.nesdev.com/w/index.php/NROM
    /// <summary> NROM: $6000-$7FFF expansion RAM for test ROMs (e.g. blargg writes result to $6000). PPU CHR from CHR-ROM only. </summary>
    internal abstract class NROM : Board {

        private readonly byte[] expansionRam = new byte[0x2000]; // $6000-$7FFF: writable for test ROM output

        protected override byte Read(int address) {
            if (address < 0x6000) {
                return 0xFF;
            }
            if (address < 0x8000) {
                return expansionRam[address - 0x6000];
            }
            if (address < 0xC000) {
                if (prg.Count > 1) {
                    return prg[1][address - 0x8000];
                }
                return prg[0][address - 0x8000];
            }
            if (address < 0x10000) {
                return prg[0][address - 0xC000];
            }
            throw new IndexOutOfRangeException();
        }

        protected override void Write(int address, byte value) {
            if (address >= 0x6000 && address < 0x8000) {
                expansionRam[address - 0x6000] = value;
            }
            // NROM: $4020-$5FFF and $8000-$FFFF writes ignored (open bus / read-only PRG).
        }

        /// <summary> NROM has CHR-ROM; PPU reads from ROM, not from $6000 CPU space. </summary>
        public override byte ReadChr(int ppuAddress) => chr[0][ppuAddress & 0x1FFF];

        /// <summary> NROM has CHR-ROM; PPU writes to 0x0000-0x1FFF are no-ops on hardware. </summary>
        public override void WriteChr(int ppuAddress, byte value) { }
    }
}