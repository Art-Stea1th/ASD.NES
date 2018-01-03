using System;

namespace ASD.NES.Kernel.ConsoleComponents.PPUParts {

    using BasicComponents;
    using Registers;
    using Shared;

    internal sealed class RegistersPPU : IMemory<Octet> {

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
        public readonly RefOctet OamAddr;

        /// <summary> OAM data register,
        /// Write OAM data here. Writes will increment OAMADDR after the write; reads during vertical or forced blanking return the value from OAM at that address but do not increment <para/>
        /// 0x2004 - (Common name: OAMDATA) </summary>
        public readonly RefOctet OamData;



        /// <summary> PPU scrolling position register <para/>
        /// 0x2005 - (Common name: PPUSCROLL)</summary>
        public readonly ScrollRegister PpuScrl;



        /// <summary> PPU address register <para/>
        /// 0x2006 - (Common name: PPUADDR) </summary>
        public readonly RefHextet PpuAddr;

        /// <summary> PPU data register <para/>
        /// 0x2007 - (Common name: PPUDATA) </summary>
        public readonly RefOctet PpuData;



        /// <summary> OAM DMA register (high octet) <para/>
        /// 0x4014 - (Common name: OAMDMA) </summary>
        public RefOctet OamDmaR;

        public event Action<Octet> OAMDMAWritten;

        public RegistersPPU() {

            PpuCtrl = new ControlRegister(RefOctet.Wrap(0));
            PpuMask = new MaskRegister(RefOctet.Wrap(0));
            PpuStat = new StatusRegister(RefOctet.Wrap(0));
            OamAddr = RefOctet.Wrap(0);
            OamData = RefOctet.Wrap(0);
            PpuScrl = new ScrollRegister(RefHextet.Wrap(0));
            PpuAddr = RefHextet.Wrap(0);
            PpuData = RefOctet.Wrap(0);
            OamDmaR = RefOctet.Wrap(0);
        }

        public Octet this[int address] {
            get => Read(address);
            set => Write(address, value);
        }

        public int Cells => 9;

        public Octet Read(int address) {

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
                    PpuData.Value = ppuMemory[readAddress];
                }
                else {
                    returnValue = ppuMemory[readAddress];
                }

                PpuAddr.Value = (ushort)(PpuAddr + PpuCtrl.IncrementPerCPURW);

                return returnValue;
            }

            throw new ArgumentOutOfRangeException($"Unimplemented read to RegisteraPPU @ {(0x2000 + address):X}");
        }

        public void Write(int address, Octet value) {

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
                    OamAddr.Value = value;
                }
                if (address == 5) {
                    PpuScrl.X = PpuScrl.Y;
                    PpuScrl.Y = value;
                }
                else if (address == 6) {
                    PpuAddr.Value.H = PpuAddr.Value.L;
                    PpuAddr.Value.L = value;
                }
                else if (address == 7) {

                    PpuData.Value = value;

                    ppuMemory[PpuAddr.Value & ppuMemory.LastAddress] = PpuData.Value;

                    PpuAddr.Value = (ushort)(PpuAddr.Value + PpuCtrl.IncrementPerCPURW);
                }
                //else {
                //    throw new ArgumentOutOfRangeException($"Unimplemented write {(byte)value:X2} to RegistersPPU @ {(0x2000 + address):X4}");
                //}
            }
        }
    }
}