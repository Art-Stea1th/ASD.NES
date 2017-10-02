using System.Collections.Generic;
using System.Linq;

namespace ASD.NESCore {

    using CartridgeParts;
    using Common;
    using Helpers;

    public sealed class Cartridge {

        private MemoryBus memory = MemoryBus.Instance;

        private Header header;
        private byte[] data;

        private List<byte[]> d;

        public static Cartridge Create(byte[] data) {
            return new Cartridge(Header.Create(data), data.Skip(16).ToArray());
        }

        private Cartridge(Header header, byte[] data) {

            this.header = header; this.data = data;

            throw new Information(
                $"File Type: {header.FileType}\n" +
                $"PRGCount: {header.PRGROMCount}\n" +
                $"CHRCount: {header.CHRROMCount}\n" +
                $"Mirroring: {header.Mirroring}\n" +
                $"Mapper: {header.MapperNumber}");
        }
    }
}