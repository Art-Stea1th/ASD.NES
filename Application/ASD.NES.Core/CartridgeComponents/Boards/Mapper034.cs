using System;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    /// <summary> BNROM / NINA-001/002 (Mapper 34). BNROM: 32K PRG switch at $8000-$FFFF, 8K CHR-RAM. NINA: registers at $7FFD/$7FFE/$7FFF, PRG-RAM $6000-$7FFF. </summary>
    /// <see href="https://www.nesdev.org/wiki/INES_Mapper_034">NESDEV INES Mapper 034</see>
    internal sealed class Mapper034 : Board {

        private readonly byte[] prgRam = new byte[0x2000];
        private int prgBank32K;
        private int chrBank0; // 4K at $0000
        private int chrBank1; // 4K at $1000
        private int numPrg32K;
        private int numChr4K;
        private bool isNina; // true = NINA (CHR-ROM), false = BNROM (CHR-RAM)
        private readonly byte[] chrRam = new byte[0x2000];

        public override void SetPRG(System.Collections.Generic.IReadOnlyList<byte[]> prg) {
            base.SetPRG(prg);
            numPrg32K = prg == null || prg.Count == 0 ? 1 : Math.Max(1, prg.Count / 2);
        }

        public override void SetCHR(System.Collections.Generic.IReadOnlyList<byte[]> chr) {
            base.SetCHR(chr);
            numChr4K = chr == null || chr.Count == 0 ? 0 : chr.Count * 2;
            isNina = numChr4K > 2;
        }

        protected override byte Read(int address) {
            if (address < 0x6000) {
                return 0;
            }
            if (address < 0x8000) {
                return prgRam[address - 0x6000];
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
            if (address >= 0xFFFA) {
                return;
            }
            if (address >= 0x6000 && address < 0x8000) {
                prgRam[address - 0x6000] = value;
                if (isNina) {
                    if (address == 0x7FFD) {
                        prgBank32K = (value & 3) % numPrg32K;
                    } else if (address == 0x7FFE) {
                        chrBank0 = (value & 15) % Math.Max(1, numChr4K);
                    } else if (address == 0x7FFF) {
                        chrBank1 = (value & 15) % Math.Max(1, numChr4K);
                    }
                }
                return;
            }
            if (address >= 0x8000) {
                prgBank32K = (value & 3) % numPrg32K;
            }
        }

        public override byte ReadChr(int ppuAddress) {
            if (isNina && chr != null && numChr4K > 0) {
                var bank4 = Math.Min(ppuAddress < 0x1000 ? chrBank0 : chrBank1, numChr4K - 1);
                var bank8 = bank4 / 2;
                var offset = (bank4 % 2) * 0x1000 + (ppuAddress & 0xFFF);
                return chr[bank8][offset];
            }
            return chrRam[ppuAddress & 0x1FFF];
        }

        public override void WriteChr(int ppuAddress, byte value) {
            if (!isNina) {
                chrRam[ppuAddress & 0x1FFF] = value;
            }
        }
    }
}
