using System.Linq;

namespace ASD.NESCore.CartridgeParts {

    using Helpers;

    internal static class Extensions {

        public static bool ContainsNESLabel(this byte[] data)
            => new string(data.Take(3).Select(c => (char)c).ToArray()) == "NES";

        public static DataFormat GetFormat(this byte[] data) {

            var expectedPRG = BitOperations.MakeInt16(data[9].LNybble(), data[4]);
            var expectedCHR = BitOperations.MakeInt16(data[9].HNybble(), data[5]);

            if (data[7].HasBit(3) && expectedPRG < data.Length) {
                return DataFormat.NES20;
            }
            if (!data[7].HasBit(3) & data[12] == 0 & data[13] == 0 & data[14] == 0 & data[15] == 0) {
                return DataFormat.INES;
            }
            return DataFormat.ArchNES;
        }
    }
}