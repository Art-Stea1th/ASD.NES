using System;

namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    using BasicComponents;
    using Helpers;
    using Registers;

    internal sealed class RegistersPPU : IMemory<byte> {

        private PPUAddressSpace ppuMemory = PPUAddressSpace.Instance;

        /// <summary> PPU control register,
        /// Various flags controlling PPU operation <para/>
        /// 0x2000 - (Common name: PPUCTRL) </summary>
        public readonly ControlRegister PpuCtrl;

        /// <summary> PPU mask register,
        /// This register controls the rendering of sprites and backgrounds, as well as colour effects <para/>
        /// 0x2001 - (Common name: PPUMASK) </summary>
        public readonly MaskRegister PpuMask;

        /// <summary> PPU status register,
        /// This register reflects the state of various functions inside the PPU <para/>
        /// 0x2002 - (Common name: PPUSTATUS) </summary>
        public readonly StatusRegister PpuStat;


        /// <summary> OAM address register,
        /// Write the address of OAM you want to access here. Most games just write 0x00 here and then use OAMDMA <para/>
        /// 0x2003 - (Common name: OAMADDR) </summary>
        public byte OamAddr { get; private set; }

        /// <summary> OAM data register,
        /// Write OAM data here. Writes will increment OAMADDR after the write; reads during vertical or forced blanking return the value from OAM at that address but do not increment <para/>
        /// 0x2004 - (Common name: OAMDATA) </summary>
        public byte OamData { get; private set; }



        /// <summary> PPU scrolling position register <para/>
        /// 0x2005 - (Common name: PPUSCROLL)</summary>
        public readonly ScrollRegister PpuScrl;



        /// <summary> PPU address register <para/>
        /// 0x2006 - (Common name: PPUADDR). Two writes: first = high byte, second = low byte. Read of $2002 resets the latch. </summary>
        public ushort PpuAddr { get; private set; }
        private bool _ppuAddrFirstWrite = true; // true = next write to $2006 is high byte
        private bool _scrollFirstWrite = true;   // true = next write to $2005 is X

        /// <summary> PPU data register <para/>
        /// 0x2007 - (Common name: PPUDATA) </summary>
        public byte PpuData { get; private set; }



        /// <summary> OAM DMA register (high octet) <para/>
        /// 0x4014 - (Common name: OAMDMA) </summary>
        public byte OamDmaR { get; private set; }

        public event Action<byte> OAMDMAWritten;

        public RegistersPPU() {

            PpuCtrl = new ControlRegister(0);
            PpuMask = new MaskRegister(0);
            PpuStat = new StatusRegister(0);
            PpuScrl = new ScrollRegister(0);
        }

        public void OnColdBoot() {

            PpuCtrl.Clear();
            PpuMask.Clear();
            PpuStat.VBlank = false;

            PpuScrl.Value = 0;
            PpuAddr = 0;
            PpuData = 0;
            _ppuAddrFirstWrite = true;
            _scrollFirstWrite = true;
        }

        public byte this[int address] {
            get => Read(address);
            set => Write(address, value);
        }

        public int Cells => 9;

        public byte Read(int address) {

            address &= 0x0007;

            if (address == 0) {
                // 0x2000 PPUCTRL — write-only on NES; read returns open bus, emulator returns last written value
                return (byte)PpuCtrl;
            }
            if (address == 1) {
                return PpuMask;
            }
            if (address == 2) {

                var prevStatusWithData = (byte)(PpuStat.StatusOnly | (PpuData & 0b0001_1111));
                PpuStat.VBlank = false;
                _ppuAddrFirstWrite = true;
                _scrollFirstWrite = true;

                return prevStatusWithData;
            }
            if (address == 3) {
                // 0x2003 OAMADDR — write-only; return last written value
                return OamAddr;
            }
            if (address == 4) {
                // 0x2004 OAMDATA — read returns OAM byte at OamAddr (or last value); no increment on read
                return OamData;
            }
            if (address == 5) {
                // 0x2005 PPUSCROLL — write-only; return first byte of scroll latch (X)
                return PpuScrl.X;
            }
            if (address == 6) {
                // 0x2006 PPUADDR — write-only; return high byte of address latch
                return (byte)(PpuAddr >> 8);
            }
            if (address == 7) {

                var readAddress = (ushort)(PpuAddr & ppuMemory.LastAddress);

                byte returnValue;

                if (readAddress < 0x3F00) {
                    returnValue = PpuData;
                    PpuData = ppuMemory[readAddress];
                }
                else {
                    returnValue = ppuMemory[readAddress];
                }

                PpuAddr = (ushort)(PpuAddr + PpuCtrl.IncrementPerCPURW);

                return returnValue;
            }

            return 0;
        }

        public void Write(int address, byte value) {

            if (address == 0x4014) {
                OAMDMAWritten(value);
            }
            else {

                address &= 0x0007;

                if (address == 0) {
                    PpuCtrl.Value = value;
                }
                else if (address == 1) {
                    PpuMask.Value = value;
                }
                if (address == 3) {
                    OamAddr = value;
                }
                if (address == 5) {
                    if (_scrollFirstWrite) {
                        PpuScrl.X = value;
                        _scrollFirstWrite = false;
                    } else {
                        PpuScrl.Y = value;
                        _scrollFirstWrite = true;
                    }
                }
                else if (address == 6) {
                    if (_ppuAddrFirstWrite) {
                        PpuAddr = (ushort)(value << 8);
                        _ppuAddrFirstWrite = false;
                    } else {
                        PpuAddr = (ushort)((PpuAddr & 0xFF00) | value);
                        _ppuAddrFirstWrite = true;
                    }
                }
                else if (address == 7) {

                    PpuData = value;

                    ppuMemory[PpuAddr & ppuMemory.LastAddress] = PpuData;

                    PpuAddr = (ushort)(PpuAddr + PpuCtrl.IncrementPerCPURW);
                }
                //else {
                //    throw new ArgumentOutOfRangeException($"Unimplemented write {(byte)value:X2} to RegistersPPU @ {(0x2000 + address):X4}");
                //}
            }
        }
    }
}