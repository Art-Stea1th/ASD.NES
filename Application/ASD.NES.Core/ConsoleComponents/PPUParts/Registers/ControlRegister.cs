namespace ASD.NES.Core.ConsoleComponents.PPUParts.Registers {

    using Shared;

    /// <summary> PPU control register,
    /// Various flags controlling PPU operation
    /// (Common name: PPUCTRL) </summary>
    internal sealed class ControlRegister {

        private readonly RefOctet r;

        /// <summary> Add 256 to the X scroll position </summary>
        public bool Add256ToX { get => r[0]; set => r[0] = value; }

        /// <summary> Add 240 to the Y scroll position </summary>
        public bool Add240ToY { get => r[1]; set => r[1] = value; }

        /// <summary> VRAM address increment per CPU read/write of PPUDATA (True: add 32, going down; False: add 1, going across) </summary>
        public bool AddressIncrement32PerCpuRW { get => r[2]; set => r[2] = value; }

        /// <summary> Sprite pattern table address for 8x8 sprites (True: 0x1000; False: 0x0000), ignored in 8x16 mode </summary>
        public bool SpriteAddress1kFor8x8 { get => r[3]; set => r[3] = value; }

        /// <summary> Background pattern table address (True: 0x1000; False: 0x0000) </summary>
        public bool BackroundAddress1k { get => r[4]; set => r[4] = value; }

        /// <summary> Sprite size 8x16 (True: 8x16; False: 8x8) </summary>
        public bool SpriteSize16 { get => r[5]; set => r[5] = value; }

        /// <summary> PPU master/slave select (True: output color on EXT pins; False: read backdrop from EXT pins) </summary>
        public bool PPUMaster { get => r[6]; set => r[6] = value; }

        /// <summary> Generate an NMI at the start of the vertical blanking interval (True: on; False: off) </summary>
        public bool NMIAtVBI { get => r[7]; set => r[7] = value; }

        public ControlRegister(RefOctet register) => r = register;
    }
}