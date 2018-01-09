namespace ASD.NES.Kernel.ConsoleComponents.PPUParts.Registers {

    using Helpers;
    using Shared;

    /// <summary> 0x2005 - PPU scrolling position register
    /// (Common name: PPUSCROLL)</summary>
    internal sealed class ScrollRegister {

        private readonly RefInt16 r;

        public byte X { get => r.Value.H(); set => r.Value = BitOperations.MakeInt16(value, r.Value.L()); }
        public byte Y { get => r.Value.L(); set => r.Value = BitOperations.MakeInt16(r.Value.H(), value); }

        public ScrollRegister(RefInt16 register) => r = register;

        public ushort Value { get => r.Value; set => r.Value = value; }

        public void Clear() => r.Value = 0;


        public static implicit operator ushort(ScrollRegister register) => register.r.Value;
    }
}