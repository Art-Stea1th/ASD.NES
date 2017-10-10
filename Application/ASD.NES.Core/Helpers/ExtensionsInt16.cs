namespace ASD.NES.Core.Helpers {

    internal static class ExtensionsInt16 {

        public static byte HOctet(this ushort int16)
            => (byte)(int16 >> 8);

        public static byte LOctet(this ushort int16)
            => (byte)int16;

    }
}