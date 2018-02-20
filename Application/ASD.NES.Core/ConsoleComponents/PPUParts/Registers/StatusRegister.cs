namespace ASD.NES.Core.ConsoleComponents.PPUParts.Registers {

    using Helpers;

    /// <summary> 0x2002 - PPU status register,
    /// This register reflects the state of various functions inside the PPU.
    /// Common name: PPUSTATUS </summary>
    internal sealed class StatusRegister {

        private byte r;
        public StatusRegister(byte register) => r = register;

        /// <summary> True: more than eight sprites appear on a scanline </summary>
        public bool SpriteOverflow { get => r.HasBit(5); set => r = r.WithChangedBit(5, value); }

        /// <summary> True: when a nonzero pixel of sprite 0 overlaps a nonzero background pixel <para/>
        /// Cleared at dot 1 of the pre-render line.Used for raster timing </summary>
        public bool SpriteZeroHit { get => r.HasBit(6); set => r = r.WithChangedBit(6, value); }

        /// <summary> Vertical blank has started (True: in vblank; False: not in vblank) <para/>
        /// Set at dot 1 of line 241 (the line *after* the post-render line); cleared after reading $2002 and at dot 1 of the pre-render line </summary>
        public bool VBlank { get => r.HasBit(7); set => r = r.WithChangedBit(7, value); }

        public byte StatusOnly => (byte)(r & 0b1110_0000);

        public void Clear() => r = 0;
        public byte Value { get => r; set => r = value; }


        public static implicit operator byte(StatusRegister register) => register.r;
    }
}