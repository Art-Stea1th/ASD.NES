namespace ASD.NESCore.Helpers {

    internal static class BitOperations {

        public static short MakeInt16(byte high, byte low)
            => (short)((high << 8) | low);

        public static byte MakeInt8(byte high, byte low)
            => (byte)((high << 4) | (low & 0b0000_1111));
    }
}