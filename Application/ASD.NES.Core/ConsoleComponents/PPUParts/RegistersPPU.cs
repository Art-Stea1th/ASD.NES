namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    using Shared;

    internal sealed class RegistersPPU {

        /// <summary> PPU control register </summary>
        public Octet PpuCtrl;
        /// <summary> PPU mask register </summary>
        public Octet PpuMask;
        /// <summary> PPU status register </summary>
        public Octet PpuStat;

        /// <summary> OAM address register </summary>
        public Octet OamAddr;
        /// <summary> OAM data register </summary>
        public Octet OamData;

        /// <summary> PPU scrolling position register </summary>
        public Octet PpuScrl;

        /// <summary> PPU address register </summary>
        public Octet PpuAddr;
        /// <summary> PPU data register </summary>
        public Octet PpuData;

        /// <summary> OAM DMA register (high octet) </summary>
        public Octet OamDmaR;
    }
}