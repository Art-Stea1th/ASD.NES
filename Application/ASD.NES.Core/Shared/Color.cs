using System.Runtime.InteropServices;

namespace ASD.NES.Core.Shared {

    [StructLayout(LayoutKind.Explicit)]
    internal struct Color {

        [FieldOffset(0)] private uint color;

        [FieldOffset(0)] private byte b;
        [FieldOffset(1)] private byte g;
        [FieldOffset(2)] private byte r;
        [FieldOffset(3)] private byte a;

        public byte A { get => a; set => a = value; }
        public byte R { get => r; set => r = value; }
        public byte G { get => g; set => g = value; }
        public byte B { get => b; set => b = value; }

        public uint BGRA { get => color; set => color = value; }

        public static implicit operator string(Color color) => color.ToString();

        public override string ToString() => $"#{color:X}";

        public Color(byte a, byte r, byte g, byte b) : this() {
            this.a = a; this.r = r; this.g = g; this.b = b;
        }

        public Color(int bgraValue) : this((uint)bgraValue) { }
        public Color(uint bgraValue) : this() => color = bgraValue;

    }
}