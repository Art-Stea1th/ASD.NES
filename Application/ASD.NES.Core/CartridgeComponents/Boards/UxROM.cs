using System;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    using Core;

    /// <summary> UxROM (UNROM/UOROM): PRG bank switch at $8000-$BFFF, fixed last bank at $C000-$FFFF. CHR: 8 KB ROM or 8 KB RAM when header CHR count = 0. NESDEV: optional bus conflict (latch = value AND PRG byte). </summary>
    /// <see href="https://www.nesdev.org/wiki/UxROM">NESDEV UxROM</see>
    internal abstract class UxROM : Board {

        private int prgBank;
        /// <summary> 8 KB CHR-RAM when iNES header reports 0 CHR banks; game uploads tiles via PPUDATA. </summary>
        private readonly byte[] chrRam = new byte[0x2000];

        protected override byte Read(int address) {
            // 0x4020–0x5FFF: expansion / open bus; not mapped by UxROM (CPUAddressSpace sends all >= 0x4020 here)
            if (address < 0x6000) {
                return 0;
            }
            if (address < 0x8000) {
                if (chr.Count > 0) {
                    return chr[0][address - 0x6000];
                }
                return chrRam[address - 0x6000];
            }
            if (address < 0xC000) {
                var bank = prgBank;
                if (bank >= prg.Count) {
                    bank = prg.Count - 1;
                }
                return prg[bank][address - 0x8000];
            }
            if (address < 0x10000) {
                return prg[prg.Count - 1][address - 0xC000];
            }
            throw new IndexOutOfRangeException();
        }

        protected override void Write(int address, byte value) {
            if (address >= 0xFFFA) {
                return; // vectors read-only
            }
            if (address >= 0x6000 && address < 0x8000) {
                if (chr.Count == 0) {
                    chrRam[address - 0x6000] = value;
                }
                return;
            }
            if (address >= 0x8000 && address < 0x10000) {
                var maxBanks = prg.Count;
                var effective = value & 0xFF;
                if (EmulationOptions.UxROMBusConflict && maxBanks > 0) {
                    var prgByte = GetPrgByteAt(address);
                    effective = (value & prgByte) & 0xFF;
                }
                prgBank = maxBanks > 1 ? effective % maxBanks : 0;
            }
        }

        private byte GetPrgByteAt(int address) {
            if (prg == null || prg.Count == 0) return 0xFF;
            if (address < 0xC000) {
                var b = prgBank >= prg.Count ? prg.Count - 1 : prgBank;
                return prg[b][address - 0x8000];
            }
            return prg[prg.Count - 1][address - 0xC000];
        }
    }
}
