using System;
using System.Collections.Generic;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    using ConsoleComponents.CPUParts;
    using ConsoleComponents.PPUParts;

    /// <summary> MMC3 (Mapper 4): PRG 8K bank switch, CHR 1K/2K bank switch, mirroring, PRG RAM, scanline IRQ. </summary>
    /// <see href="https://www.nesdev.org/wiki/MMC3">NESDEV MMC3</see>
    internal abstract class MMC3 : Board {

        private byte bankSelect;
        private readonly byte[] regs = new byte[8];
        private readonly byte[] prgRam = new byte[0x2000]; // 8 KB at $6000-$7FFF
        private bool prgRamEnabled;
        private bool prgRamWriteProtected;
        private int numPrg8K;
        private int numChr1K;
        private int irqCounter;
        private byte irqLatch;
        private bool irqEnabled;

        protected override byte Read(int address) {
            if (address < 0x6000)
                return 0;
            if (address < 0x8000) {
                if (!prgRamEnabled) return 0;
                return prgRam[address - 0x6000];
            }
            if (address < 0x10000)
                return GetPrgByte(address);
            return 0;
        }

        protected override void Write(int address, byte value) {
            if (address >= 0xFFFA) return;
            if (address >= 0x6000 && address < 0x8000) {
                if (prgRamEnabled && !prgRamWriteProtected)
                    prgRam[address - 0x6000] = value;
                return;
            }
            if (address < 0x8000) return;
            switch (address & 0xE001) {
                case 0x8000: bankSelect = value; return;
                case 0x8001: regs[bankSelect & 7] = value; return;
                case 0xA000:
                    // NESDEV: bit 0 = 0 → horizontal (CIRAM A10 = PPU A10), 1 → vertical (CIRAM A10 = PPU A11)
                    PPUAddressSpace.Instance.NametableMirroring = (value & 1) != 0 ? Mirroring.Vertical : Mirroring.Horizontal;
                    return;
                case 0xA001:
                    prgRamEnabled = (value & 0x80) != 0;
                    prgRamWriteProtected = (value & 0x40) == 0;
                    return;
                case 0xC000: irqLatch = value; return;
                case 0xC001: irqCounter = irqLatch; return;
                case 0xE000: irqEnabled = false; CPUAddressSpace.Instance.Irq = false; return;
                case 0xE001: irqEnabled = true; return;
            }
        }

        public override void OnScanline() {
            if (irqCounter == 0 && irqEnabled)
                CPUAddressSpace.Instance.Irq = true;
            if (irqCounter == 0)
                irqCounter = irqLatch;
            else
                irqCounter--;
        }

        public override void SetPRG(IReadOnlyList<byte[]> prg) {
            base.SetPRG(prg);
            numPrg8K = prg == null ? 0 : Math.Max(1, prg.Count * 2);
        }

        public override void SetCHR(IReadOnlyList<byte[]> chr) {
            base.SetCHR(chr);
            numChr1K = chr == null ? 0 : Math.Max(1, chr.Count * 8);
        }

        private byte GetPrgByte(int address) {
            if (prg == null || prg.Count == 0) return 0;
            var slot = (address >> 13) & 3; // 0=$8000, 1=$A000, 2=$C000, 3=$E000
            bool prgMode = (bankSelect & 0x40) != 0;
            int bank8;
            if (!prgMode) {
                switch (slot) {
                    case 0: bank8 = regs[6] & 0x3F; break;
                    case 1: bank8 = regs[7] & 0x3F; break;
                    case 2: bank8 = numPrg8K - 2; break;
                    default: bank8 = numPrg8K - 1; break;
                }
            } else {
                switch (slot) {
                    case 0: bank8 = numPrg8K - 2; break;
                    case 1: bank8 = regs[7] & 0x3F; break;
                    case 2: bank8 = regs[6] & 0x3F; break;
                    default: bank8 = numPrg8K - 1; break;
                }
            }
            if (bank8 >= numPrg8K) bank8 = numPrg8K - 1;
            var offset = address & 0x1FFF;
            var lo = prg[bank8 / 2];
            return lo[(bank8 % 2) * 0x2000 + offset];
        }

        public override byte ReadChr(int ppuAddress) {
            if (chr == null || chr.Count == 0) return 0;
            var offset = ppuAddress & 0x1FFF;
            bool chrMode = (bankSelect & 0x80) != 0;
            int bank1K;
            int off1K;
            if (!chrMode) {
                if (offset < 0x0800) { bank1K = (regs[0] & 0xFE) + (offset >> 10); off1K = offset & 0x3FF; }
                else if (offset < 0x1000) { bank1K = (regs[1] & 0xFE) + ((offset - 0x800) >> 10); off1K = (offset - 0x800) & 0x3FF; }
                else { bank1K = regs[2 + ((offset - 0x1000) >> 10)] & 0xFF; off1K = offset & 0x3FF; }
            } else {
                if (offset < 0x1000) { bank1K = regs[2 + (offset >> 10)] & 0xFF; off1K = offset & 0x3FF; }
                else if (offset < 0x1800) { bank1K = (regs[0] & 0xFE) + ((offset - 0x1000) >> 10); off1K = (offset - 0x1000) & 0x3FF; }
                else { bank1K = (regs[1] & 0xFE) + ((offset - 0x1800) >> 10); off1K = (offset - 0x1800) & 0x3FF; }
            }
            if (bank1K >= numChr1K) bank1K = numChr1K - 1;
            var chunk = chr[bank1K / 8];
            return chunk[(bank1K % 8) * 0x400 + off1K];
        }

        public override void WriteChr(int ppuAddress, byte value) { }
    }
}
