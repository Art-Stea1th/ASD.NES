using System;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    /// <summary> GxROM (Mapper 66): 32 KB PRG + 8 KB CHR bank switch via $8000-$FFFF. GNROM/MHROM. </summary>
    /// <see href="https://www.nesdev.org/wiki/GxROM">NESDEV GxROM</see>
    internal sealed class Mapper066 : Board {

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
            if (address < 0x8000) {
                return;
            }
            if (address >= 0xFFFA) {
                return;
            }
            var pp = (value >> 4) & 3;
            var cc = value & 3;
            prgBank32K = pp % numPrg32K;
            chrBank8K = cc % numChr8K;
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
