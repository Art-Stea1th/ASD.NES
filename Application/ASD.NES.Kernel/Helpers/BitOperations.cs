using System.Linq;

namespace ASD.NES.Kernel.Helpers {

    using Shared;

    internal static class BitOperations {

        public static ushort L(this uint value) => (ushort)value;
        public static ushort H(this uint value) => (ushort)(value >> 16);

        public static byte LL(this uint value) => (byte)(value);
        public static byte LH(this uint value) => (byte)(value >> 8);
        public static byte HL(this uint value) => (byte)(value >> 16);
        public static byte HH(this uint value) => (byte)(value >> 24);

        public static byte L(this ushort value) => (byte)value;
        public static byte H(this ushort value) => (byte)(value >> 8);

        public static byte L(this byte value) => (byte)(value & 0xF);
        public static byte H(this byte value) => (byte)(value >> 4);

        public static uint MakeInt32(ushort highOctet, ushort lowOctet)
            => (uint)((highOctet << 16) | (lowOctet & 0xFFFF));

        public static ushort MakeInt16(byte highOctet, byte lowOctet)
            => (ushort)((highOctet << 8) | (lowOctet & 0xFF));

        public static byte MakeInt8(byte highNybble, byte lowNybble)
            => (byte)((highNybble << 4) | (lowNybble & 0xF));

        public static byte MakeInt8(bool bit7, bool bit6, bool bit5, bool bit4, bool bit3, bool bit2, bool bit1, bool bit0) {
            return (byte)(
                ((bit7 ? 1 : 0) << 7) | ((bit6 ? 1 : 0) << 6) | ((bit5 ? 1 : 0) << 5) | ((bit4 ? 1 : 0) << 4) |
                ((bit3 ? 1 : 0) << 3) | ((bit2 ? 1 : 0) << 2) | ((bit1 ? 1 : 0) << 1) | ((bit0 ? 1 : 0) << 0)); // give me simple bool cast :\
        }

        public static bool HasBit(this byte n, int bit) => ((int)n).HasBit(bit);
        public static bool HasBit(this sbyte n, int bit) => ((int)n).HasBit(bit);
        public static bool HasBit(this ushort n, int bit) => ((int)n).HasBit(bit);
        public static bool HasBit(this short n, int bit) => ((int)n).HasBit(bit);
        public static bool HasBit(this uint n, int bit) => ((int)n).HasBit(bit);
        public static bool HasBit(this int n, int bit) => ((n >> bit) & 1) == 1;

        public static byte WithChangedBit(this byte n, int bit, bool value)
            => (byte)((int)n).WithChangedBit(bit, value);
        public static sbyte WithChangedBit(this sbyte n, int bit, bool value)
            => (sbyte)((int)n).WithChangedBit(bit, value);
        public static ushort WithChangedBit(this ushort n, int bit, bool value)
            => (ushort)((int)n).WithChangedBit(bit, value);
        public static short WithChangedBit(this short n, int bit, bool value)
            => (short)((int)n).WithChangedBit(bit, value);
        public static uint WithChangedBit(this uint n, int bit, bool value)
            => (uint)((int)n).WithChangedBit(bit, value);
        public static int WithChangedBit(this int n, int bit, bool value)
            => value ? (n | (1 << bit)) : (n & ~(1 << bit));

    }

    internal static class RefExtensions { // tmp

        public static RefInt32[] Wrap(this uint[] array)
            => array.Select(b => RefInt32.Wrap(b)).ToArray();
        public static RefInt16[] Wrap(this ushort[] array)
            => array.Select(b => RefInt16.Wrap(b)).ToArray();
        public static RefInt8[] Wrap(this byte[] array)
            => array.Select(b => RefInt8.Wrap(b)).ToArray();
    }
}