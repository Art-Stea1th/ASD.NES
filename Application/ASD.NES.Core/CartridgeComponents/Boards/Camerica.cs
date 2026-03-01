using System;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    using ConsoleComponents.PPUParts;

    /// <summary> Camerica (Mapper 71): UNROM-like, 16K PRG switch at $8000-$BFFF via $C000-$FFFF. CHR 8K (fixed). Optional 1-screen mirroring at $9000. </summary>
    /// <see href="https://www.nesdev.org/wiki/Camerica">NESDEV INES Mapper 071</see>
    internal sealed class Mapper071 : Board {

        private int prgBank;
        private readonly byte[] chrRam = new byte[0x2000];

        protected override byte Read(int address) {
            if (address < 0x6000) {
                return 0;
            }
            if (address < 0x8000) {
                if (chr != null && chr.Count > 0) {
                    return chr[0][address - 0x6000];
                }
                return chrRam[address - 0x6000];
            }
            if (address < 0xC000) {
                var b = Math.Min(prgBank, prg == null ? 0 : prg.Count - 1);
                return prg != null && prg.Count > 0 ? prg[b][address - 0x8000] : (byte)0;
            }
            if (address < 0x10000) {
                var last = prg != null && prg.Count > 0 ? prg.Count - 1 : 0;
                return prg != null ? prg[last][address - 0xC000] : (byte)0;
            }
            return 0;
        }

        protected override void Write(int address, byte value) {
            if (address >= 0xFFFA) {
                return;
            }
            if (address >= 0x6000 && address < 0x8000) {
                if (chr == null || chr.Count == 0) {
                    chrRam[address - 0x6000] = value;
                }
                return;
            }
            if (address >= 0x9000 && address < 0xA000) {
                PPUAddressSpace.Instance.NametableMirroring = Mirroring.SingleScreen;
                PPUAddressSpace.Instance.SingleScreenPage = (value & 1) != 0 ? 1 : 0;
                return;
            }
            if (address >= 0xC000 && address < 0x10000) {
                var max = prg == null ? 1 : Math.Max(1, prg.Count);
                prgBank = (value & 0x0F) % max;
            }
        }

        public override byte ReadChr(int ppuAddress) {
            if (chr != null && chr.Count > 0) {
                return chr[0][ppuAddress & 0x1FFF];
            }
            return chrRam[ppuAddress & 0x1FFF];
        }

        public override void WriteChr(int ppuAddress, byte value) {
            if (chr == null || chr.Count == 0) {
                chrRam[ppuAddress & 0x1FFF] = value;
            }
        }
    }
}
