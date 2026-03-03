using System;


namespace ASD.NES.Core.CartridgeComponents.Boards {

    // https://wiki.nesdev.com/w/index.php/NROM
    internal abstract class NROM : Board {

        protected override byte Read(int address) {

            if (address < 0x6000) {
                throw new IndexOutOfRangeException();
            }
            if (address < 0x8000) {
                return chr[0][address - 0x6000];
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
            // 0xFFFA-0xFFFF are vectors (NMI, RESET, IRQ) — read-only in cartridge ROM
            if (address >= 0xFFFA) {
                return;
            }
            throw new InvalidOperationException();
        }

        /// <summary> NROM has CHR-ROM; PPU writes to 0x0000-0x1FFF are no-ops on hardware. </summary>
        public override void WriteChr(int ppuAddress, byte value) { }
    }
}