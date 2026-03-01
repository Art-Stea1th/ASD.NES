using System;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    using ConsoleComponents.PPUParts;

    /// <summary> MMC1 (Mapper 1): serial register, PRG 16/32K bank switch, CHR 4/8K, mirroring, optional PRG RAM. </summary>
    /// <see href="https://www.nesdev.org/wiki/MMC1">NESDEV MMC1</see>
    internal abstract class MMC1 : Board {

        private int shiftRegister;
        private int shiftCount;
        private byte control = 0x0C;  // $8000-$9FFF: power-on = PRG mode 3 (fix last bank at $C000)
        private byte chrBank0;  // $A000-$BFFF
        private byte chrBank1;  // $C000-$DFFF
        private byte prgBank;  // $E000-$FFFF (bit 4 = PRG RAM enable on MMC1B)

        private readonly byte[] prgRam = new byte[0x2000]; // 8 KB at $6000-$7FFF
        private int numPrg16K;
        private int numChr8K;

        protected override byte Read(int address) {
            if (address < 0x6000) {
                return 0;
            }
            if (address < 0x8000) {
                if (!PrgRamEnabled) {
                    return 0;
                }
                return prgRam[address - 0x6000];
            }
            return GetPrgByte(address);
        }

        protected override void Write(int address, byte value) {
            if (address >= 0xFFFA) {
                return;
            }
            if (address >= 0x6000 && address < 0x8000) {
                if (PrgRamEnabled) {
                    prgRam[address - 0x6000] = value;
                }
                return;
            }
            if (address < 0x8000) {
                return;
            }

            if ((value & 0x80) != 0) {
                shiftRegister = 0;
                shiftCount = 0;
                control |= 0x0C; // lock last PRG bank at $C000
                return;
            }

            shiftRegister = (shiftRegister >> 1) | ((value & 1) << 4);
            shiftCount++;
            if (shiftCount < 5) {
                return;
            }

            var reg = (address >> 13) & 3;
            var v = shiftRegister & 0x1F;
            shiftRegister = 0;
            shiftCount = 0;

            switch (reg) {
                case 0:
                    control = (byte)v;
                    ApplyMirroring();
                    break;
                case 1:
                    chrBank0 = (byte)v;
                    break;
                case 2:
                    chrBank1 = (byte)v;
                    break;
                case 3:
                    prgBank = (byte)v;
                    break;
            }
        }

        private bool PrgRamEnabled => (prgBank & 0x10) == 0; // MMC1B: bit 4 = 0 enable

        private void ApplyMirroring() {
            var ppu = PPUAddressSpace.Instance;
            var m = control & 3;
            if (m <= 1) {
                ppu.NametableMirroring = Mirroring.SingleScreen;
                ppu.SingleScreenPage = m;
            } else if (m == 2) {
                ppu.NametableMirroring = Mirroring.Vertical;
            } else {
                ppu.NametableMirroring = Mirroring.Horizontal;
            }
        }

        public override void SetPRG(System.Collections.Generic.IReadOnlyList<byte[]> prg) {
            base.SetPRG(prg);
            numPrg16K = prg == null ? 0 : Math.Max(1, prg.Count);
        }

        public override void SetCHR(System.Collections.Generic.IReadOnlyList<byte[]> chr) {
            base.SetCHR(chr);
            numChr8K = chr == null ? 0 : Math.Max(1, chr.Count);
        }

        private byte GetPrgByte(int address) {
            if (prg == null || prg.Count == 0) {
                return 0;
            }
            var prgMode = (control >> 2) & 3;
            int bank;
            if (address < 0xC000) {
                if (prgMode == 2) {
                    bank = 0;
                } else if (prgMode == 3) {
                    bank = (prgBank & 0x0F) % numPrg16K;
                } else {
                    bank = ((prgBank & 0x0F) & 0xFE) % numPrg16K;
                }
            } else {
                if (prgMode == 2) {
                    bank = (prgBank & 0x0F) % numPrg16K;
                } else {
                    bank = numPrg16K - 1;
                }
            }
            if (bank >= numPrg16K) {
                bank = numPrg16K - 1;
            }
            var offset = address & 0x3FFF;
            return prg[bank][offset];
        }

        public override byte ReadChr(int ppuAddress) {
            if (chr == null || chr.Count == 0) {
                return 0;
            }
            var chr8K = (control & 0x10) == 0;
            int bank8;
            int offset;
            if (chr8K) {
                bank8 = (chrBank0 & 0x1E) >> 1;
                offset = ppuAddress & 0x1FFF;
            } else {
                var bank4 = ppuAddress < 0x1000 ? (chrBank0 >> 1) : (chrBank1 >> 1);
                bank8 = bank4 >> 1;
                offset = (bank4 & 1) * 0x1000 + (ppuAddress & 0x0FFF);
            }
            if (bank8 >= numChr8K) {
                bank8 = numChr8K - 1;
            }
            return chr[bank8][offset];
        }

        public override void WriteChr(int ppuAddress, byte value) { }
    }

    internal sealed class Mapper001 : MMC1 { }
}
