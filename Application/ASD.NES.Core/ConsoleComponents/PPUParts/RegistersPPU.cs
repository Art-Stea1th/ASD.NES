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



        /// <summary> PPU scrolling position register <para/>
        /// 0x2005 - (Common name: PPUSCROLL)</summary>
        public readonly RefOctet PpuScrl;



        /// <summary> PPU address register <para/>
        /// 0x2006 - (Common name: PPUADDR) </summary>
        public readonly RefOctet PpuAddr;

        /// <summary> PPU data register <para/>
        /// 0x2007 - (Common name: PPUDATA) </summary>
        public readonly RefOctet PpuData;



        /// <summary> OAM DMA register (high octet) <para/>
        /// 0x4014 - (Common name: OAMDMA) </summary>
        public RefOctet OamDmaR;

        public RegistersPPU() {
            PpuCtrl = new ControlRegister(OldCode.OldMemoryBus.Instance.GetReference(0x2000));
            PpuMask = new MaskRegister(OldCode.OldMemoryBus.Instance.GetReference(0x2001));
            PpuStat = new StatusRegister(OldCode.OldMemoryBus.Instance.GetReference(0x2002));
            OamAddr = OldCode.OldMemoryBus.Instance.GetReference(0x2003);
            OamData = OldCode.OldMemoryBus.Instance.GetReference(0x2004);
            PpuScrl = OldCode.OldMemoryBus.Instance.GetReference(0x2005);
            PpuAddr = OldCode.OldMemoryBus.Instance.GetReference(0x2006);
            PpuData = OldCode.OldMemoryBus.Instance.GetReference(0x2007);
            OamDmaR = OldCode.OldMemoryBus.Instance.GetReference(0x4014);
        }
    }
}