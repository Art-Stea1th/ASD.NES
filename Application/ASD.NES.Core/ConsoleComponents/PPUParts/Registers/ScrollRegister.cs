namespace ASD.NES.Core.ConsoleComponents.PPUParts.Registers {

    using Shared;

    /// <summary> 0x2005 - PPU scrolling position register
    /// (Common name: PPUSCROLL)</summary>
    internal sealed class ScrollRegister {

        private readonly RefHextet r;

        public Octet X { get => r.Value.H; set => r.Value.H = value; }
        public Octet Y { get => r.Value.L; set => r.Value.L = value; }

        public ScrollRegister(RefHextet register) => r = register;

        public Hextet Value { get => r.Value; set => r.Value = value; }

        public void Clear() => r.Value = 0;

        public static implicit operator Hextet(ScrollRegister register) => register.r.Value;
        public static implicit operator ushort(ScrollRegister register) => register.r.Value;
    }
}