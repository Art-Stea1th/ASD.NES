using System;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    /// <summary> NINA-003/006 (Mapper 79): register at $4100-$5FFF (pattern 010x xxx1). 32K PRG + 8K CHR. </summary>
    /// <see href="https://www.nesdev.org/wiki/NINA-003-006">NESDEV NINA-003-006</see>
    internal sealed class Mapper079 : Board {

        private int prgBank32K;
        private int chrBank8K;
        private int numPrg32K;
        private int numChr8K;

        public override void SetPRG(System.Collections.Generic.IReadOnlyList<byte[]> prg) {
            base.SetPRG(prg);
            numPrg32K = prg == null || prg.Count == 0 ? 1 : Math.Max(1, prg.Count / 2);
        }

        public override void SetCHR(System.Collections.Generic.IReadOnlyList<byte[]> chr) {
            base.SetCHR(chr);
            numChr8K = chr == null || chr.Count == 0 ? 1 : Math.Max(1, chr.Count);
        }

        protected override byte Read(int address) {
            if (address < 0x8000) {
                return 0;
            }
            if (prg == null || prg.Count == 0) {
                return 0;
            }
            var bankLo = Math.Min(prgBank32K * 2, prg.Count - 1);
            var bankHi = Math.Min(prgBank32K * 2 + 1, prg.Count - 1);
            var offset = address < 0xC000 ? address - 0x8000 : address - 0xC000;
            var bankIndex = address < 0xC000 ? bankLo : bankHi;
            return prg[bankIndex][offset];
        }

        protected override void Write(int address, byte value) {
            if (address >= 0x4100 && address <= 0x5FFF && (address & 0x4100) == 0x4100) {
                prgBank32K = ((value >> 3) & 1) % numPrg32K;
                chrBank8K = (value & 7) % numChr8K;
            }
        }

        public override byte ReadChr(int ppuAddress) {
            if (chr == null || chr.Count == 0) {
                return 0;
            }
            var bank = Math.Min(chrBank8K, chr.Count - 1);
            return chr[bank][ppuAddress & 0x1FFF];
        }

        public override void WriteChr(int ppuAddress, byte value) { }
    }
}
