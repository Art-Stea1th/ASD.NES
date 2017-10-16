namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    using Registers;
    using Shared;

    internal sealed class RegistersPPU {

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



        /// <summary> PPU scrolling position register </summary>
        public RefOctet PpuScrl;



        /// <summary> PPU address register </summary>
        public RefOctet PpuAddr;

        /// <summary> PPU data register </summary>
        public RefOctet PpuData;



        /// <summary> OAM DMA register (high octet) </summary>
        public RefOctet OamDmaR;

        public RegistersPPU() {

        }
    }
}