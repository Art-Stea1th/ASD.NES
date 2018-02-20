namespace ASD.NES.Core.ConsoleComponents.PPUParts.Registers {

    using Helpers;

    /// <summary> 0x2000 - PPU control register,
    /// Various flags controlling PPU operation
    /// (Common name: PPUCTRL) </summary>
    internal sealed class ControlRegister {

        private byte r;
        public ControlRegister(byte register) => r = register;

        /// <summary> Add 256 to the X scroll position </summary>
        public bool Add256ToX { get => r.HasBit(0); set => r.WithChangedBit(0, value); }

        /// <summary> Add 240 to the Y scroll position </summary>
        public bool Add240ToY { get => r.HasBit(1); set => r.WithChangedBit(1, value); }

        /// <summary> VRAM address increment per CPU read/write of PPUDATA (True: add 32, going down; False: add 1, going across) </summary>
        public bool AddressIncrement32PerCpuRW { get => r.HasBit(2); set => r.WithChangedBit(2, value); }

        /// <summary> Sprite pattern table address for 8x8 sprites (True: 0x1000; False: 0x0000), ignored in 8x16 mode </summary>
        public bool SpriteAddress1kFor8x8 { get => r.HasBit(3); set => r.WithChangedBit(3, value); }

        /// <summary> Background pattern table address (True: 0x1000; False: 0x0000) </summary>
        public bool BackroundAddress1k { get => r.HasBit(4); set => r.WithChangedBit(4, value); }

        /// <summary> Sprite size 8x16 (True: 8x16; False: 8x8) </summary>
        public bool SpriteSize16 { get => r.HasBit(5); set => r.WithChangedBit(5, value); }

        /// <summary> PPU master/slave select (True: output color on EXT pins; False: read backdrop from EXT pins) </summary>
        public bool PPUMaster { get => r.HasBit(6); set => r.WithChangedBit(6, value); }

        /// <summary> Generate an NMI at the start of the vertical blanking interval (True: on; False: off) </summary>
        public bool NMIAtVBI { get => r.HasBit(7); set => r.WithChangedBit(7, value); }

        // -----

        /// <summary> X scroll start position (0 or 256) </summary>
        public int StartX => (r & 0b01) << 8;

        /// <summary> Y scroll start position (0 or 240) </summary>
        public int StartY => ((r & 0b10) << 6) | ((r & 0b10) << 5) | ((r & 0b10) << 4) | ((r & 0b10) << 3);

        /// <summary> Base nametable address (0x2000; 0x2400; 0x2800; 0x2C00) </summary>
        public int NametableAddress => 0x2000 | ((r & 0b11) << 10);


        public int IncrementPerCPURW => r.HasBit(2) ? 32 : 1;

        /// <summary> Sprite pattern table address for 8x8 sprites (0x0000 or 0x1000), ignored in 8x16 mode </summary>
        public int SpritePatternTableAddress => (r & 0b0_1000) << 9;

        /// <summary> Background pattern table address (0x0000 or 0x1000) </summary>
        public int BackgroundPatternTableAddress => (r & 0b1_0000) << 8;

        /// <summary> Sprite width: always 8 px </summary>
        public byte SpriteSizeX => 8;

        /// <summary> Sprite height: 8 or 16 px </summary>
        public byte SpriteSizeY => (byte)(8 << ((r & 0b10_0000) >> 5));

        public byte Value { get => r; set => r = value; }

        public void Clear() => r = 0;

        public static implicit operator byte(ControlRegister register) => register.r;
    }
}