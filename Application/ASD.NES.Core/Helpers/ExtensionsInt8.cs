namespace ASD.NES.Core.Helpers {

    internal static class ExtensionsInt8 {

        // public

        public static byte HNybble(this byte int8)
            => (byte)((0b1111_0000 & int8) >> 4);

        public static byte LNybble(this byte int8)
            => (byte)(0b0000_1111 & int8);

        public static bool HasBit(this byte int8, int bit)
            => ((int8 >> bit) & 1) == 1;

        public static byte WithChangedBit(this byte int8, int bit, bool value)
            => value ? WithBit(int8, bit) : WithoutBit(int8, bit);

        // private

        private static byte WithBit(byte int8, int bit)
            => (byte)(int8 | (1 << bit));

        private static byte WithoutBit(byte int8, int bit)
            => (byte)(int8 & ~(1 << bit));

    }
}