using System;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    using ConsoleComponents.PPUParts;

    /// <summary> AxROM (Mapper 7): 32 KB switchable PRG at $8000-$FFFF, 8 KB CHR-RAM. Single-screen mirroring selectable by register bit 4. </summary>
    /// <see href="https://www.nesdev.org/wiki/AxROM">NESDEV AxROM</see>
    internal abstract class AxROM : Board {

        private int prgBank;
        private readonly byte[] chrRam = new byte[0x2000];

        protected override byte Read(int address) {
            if (address < 0x6000)
                return 0;
            if (address < 0x8000)
                return chrRam[address - 0x6000];
            if (address < 0x10000) {
                var num32Banks = Math.Max(1, prg.Count / 2);
                var bank32 = prgBank >= num32Banks ? num32Banks - 1 : prgBank;
                var offset = address - 0x8000;
                var lo = bank32 * 2;
                var hi = bank32 * 2 + 1;
                if (hi >= prg.Count) hi = lo;
                if (offset < 0x4000)
                    return prg[lo][offset];
                return prg[hi][offset - 0x4000];
            }
            return 0;
        }

        protected override void Write(int address, byte value) {
            if (address >= 0xFFFA) return;
            if (address >= 0x6000 && address < 0x8000) {
                chrRam[address - 0x6000] = value;
                return;
            }
            if (address >= 0x8000 && address < 0x10000) {
                var num32Banks = prg.Count / 2;
                if (num32Banks <= 0) num32Banks = 1;
                prgBank = (value & 0x0F) % num32Banks;
                var ppu = PPUAddressSpace.Instance;
                ppu.NametableMirroring = Mirroring.SingleScreen;
                ppu.SingleScreenPage = (value & 0x10) != 0 ? 1 : 0;
            }
        }
    }
}
