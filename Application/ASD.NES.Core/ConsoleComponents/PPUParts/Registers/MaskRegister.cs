namespace ASD.NES.Core.ConsoleComponents.PPUParts.Registers {

    using Helpers;

    /// <summary> 0x2001 - PPU mask register,
    /// This register controls the rendering of sprites and backgrounds, as well as colour effects
    /// (Common name: PPUMASK) </summary>
    internal sealed class MaskRegister {

        private byte r;
        public MaskRegister(byte register) => r = register;

        /// <summary> True: produce a greyscale display, False: normal color </summary>
        public bool Greyscale { get => r.HasBit(0); set => r.WithChangedBit(0, value); }

        /// <summary> True: need render background in leftmost 8 pixels of screen </summary>
        public bool RenderLeftmostBG { get => r.HasBit(1); set => r.WithChangedBit(1, value); }

        /// <summary> True: need render sprites in leftmost 8 pixels of screen </summary>
        public bool RenderLeftmostSpr { get => r.HasBit(2); set => r.WithChangedBit(2, value); }

        /// <summary> True: need render background </summary>
        public bool RenderBackground { get => r.HasBit(3); set => r.WithChangedBit(3, value); }

        /// <summary> True: need render sprites </summary>
        public bool RenderSprites { get => r.HasBit(4); set => r.WithChangedBit(4, value); }

        /// <summary> Emphasize red, NTSC colors. PAL and Dendy swaps green and red </summary>
        public bool EmphasizeRed { get => r.HasBit(5); set => r.WithChangedBit(5, value); }

        /// <summary> Emphasize green, NTSC colors. PAL and Dendy swaps green and red </summary>
        public bool EmphasizeGreen { get => r.HasBit(6); set => r.WithChangedBit(6, value); }

        /// <summary> Emphasize blue </summary>
        public bool EmphasizeBlue { get => r.HasBit(7); set => r.WithChangedBit(7, value); }

        // -----

        public bool RenderAll => (r & 0b0001_1110) == 0b0001_1110;

        public byte Value { get => r; set => r = value; }
        public void Clear() => r = 0;

        public static implicit operator byte(MaskRegister register) => register.r;
    }
}