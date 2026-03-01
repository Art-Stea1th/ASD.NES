using System;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    //https://wiki.nesdev.com/w/index.php/CNROM
    internal abstract class CNROM : Board { // TODO: Verify

        private int bank = 0;

        protected override byte Read(int address) {
            if (address < 0x6000) {
                return 0;
            }
            if (address < 0x8000) {
                if (chr == null || chr.Count == 0) {
                    return 0;
                }
                var b = Math.Min(bank, chr.Count - 1);
                return chr[b][address - 0x6000];
            }
            if (address < 0xC000) {
                if (prg == null || prg.Count == 0) {
                    return 0;
                }
                return prg[0][address - 0x8000];
            }
            if (address < 0x10000) {
                if (prg == null || prg.Count == 0) {
                    return 0;
                }
                return prg[prg.Count - 1][address - 0xC000];
            }
            return 0;
        }

        protected override void Write(int address, byte value) {
            if (address >= 0xFFFA) {
                return; // vectors are read-only
            }
            if (address >= 0x8000 && address <= 0xFFFF) {
                bank = value & 3;
            }
        }
    }
}