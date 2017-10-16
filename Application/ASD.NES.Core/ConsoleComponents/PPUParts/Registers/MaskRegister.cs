namespace ASD.NES.Core.ConsoleComponents.PPUParts.Registers {

    using Shared;

    /// <summary> 0x2001 - PPU mask register,
    /// This register controls the rendering of sprites and backgrounds, as well as colour effects
    /// (Common name: PPUMASK) </summary>
    internal sealed class MaskRegister {

        private readonly RefOctet r;

        /// <summary> True: produce a greyscale display, False: normal color </summary>
        public bool Greyscale { get => r[0]; set => r[0] = value; }

        /// <summary> True: show background in leftmost 8 pixels of screen, False: hide </summary>
        public bool ShowLeftmostBG { get => r[1]; set => r[1] = value; }

        /// <summary> True: show sprites in leftmost 8 pixels of screen, False: hide </summary>
        public bool ShowLeftmostSpr { get => r[2]; set => r[2] = value; }

        /// <summary> True: show background, False: hide </summary>
        public bool ShowBackground { get => r[3]; set => r[3] = value; }

        /// <summary> True: show background, False: hide </summary>
        public bool ShowSprites { get => r[4]; set => r[4] = value; }

        /// <summary> Emphasize red, NTSC colors. PAL and Dendy swaps green and red </summary>
        public bool EmphasizeRed { get => r[5]; set => r[5] = value; }

        /// <summary> Emphasize green, NTSC colors. PAL and Dendy swaps green and red </summary>
        public bool EmphasizeGreen { get => r[6]; set => r[6] = value; }

        /// <summary> Emphasize blue </summary>
        public bool EmphasizeBlue { get => r[7]; set => r[7] = value; }

        public MaskRegister(RefOctet register) => r = register;
    }
}