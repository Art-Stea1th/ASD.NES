using System;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    using Core;

    // https://wiki.nesdev.com/w/index.php/CNROM — submapper 1 = no conflict, submapper 2 = AND-type bus conflict
    internal abstract class CNROM : Board {

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
                var effective = value & 3;
                if (EmulationOptions.CNROMBusConflict && prg != null && prg.Count > 0) {
                    var prgByte = address < 0xC000 ? prg[0][address - 0x8000] : prg[prg.Count - 1][address - 0xC000];
                    effective = (value & prgByte) & 3;
                }
                bank = effective;
            }
        }
    }
}