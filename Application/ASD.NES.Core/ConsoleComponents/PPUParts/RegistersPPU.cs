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
        /// 0x2006 - (Common name: PPUADDR) </summary>
        public ushort PpuAddr { get; private set; }

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
        }

        public byte this[int address] {
            get => Read(address);
            set => Write(address, value);
        }

        public int Cells => 9;

        public byte Read(int address) {

            address &= 0x0007;

            if (address == 1) {
                return PpuMask;
            }
            else if (address == 2) {

                var prevStatusWithData = (byte)(PpuStat.StatusOnly | (PpuData & 0b0001_1111));
                PpuStat.VBlank = false;

                return prevStatusWithData;
            }
            else if (address == 7) {

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

            throw new ArgumentOutOfRangeException($"Unimplemented read to RegisteraPPU @ {(0x2000 + address):X}");
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
                    PpuScrl.X = PpuScrl.Y;
                    PpuScrl.Y = value;
                }
                else if (address == 6) {
                    PpuAddr = BitOperations.MakeInt16(PpuAddr.L(), value);
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