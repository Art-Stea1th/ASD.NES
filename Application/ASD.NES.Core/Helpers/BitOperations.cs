namespace ASD.NES.Core.Helpers {

    internal static class BitOperations {

        public static ushort MakeInt16(byte highOctet, byte lowOctet)
            => (ushort)((highOctet << 8) | lowOctet);

        public static byte MakeInt8(byte highNybble, byte lowNybble)
            => (byte)((highNybble << 4) | (lowNybble & 0b0000_1111));
    }
}