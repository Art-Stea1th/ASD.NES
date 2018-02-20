namespace ASD.NES.Core.ConsoleComponents.PPUParts.Registers {

    using Helpers;

    /// <summary> 0x2005 - PPU scrolling position register
    /// (Common name: PPUSCROLL)</summary>
    internal sealed class ScrollRegister {

        private ushort r;

        public byte X { get => r.H(); set => r = BitOperations.MakeInt16(value, r.L()); }
        public byte Y { get => r.L(); set => r = BitOperations.MakeInt16(r.H(), value); }

        public ScrollRegister(byte register) => r = register;

        public ushort Value { get => r; set => r = value; }

        public void Clear() => r = 0;

        public static implicit operator ushort(ScrollRegister register) => register.r;
    }
}