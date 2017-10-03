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

                $"File Type: {header.DataFormat}\n\n" +

                $"PRGROMs: {header.PRGROMSize}\n" +
                $"CHRROMs: {header.CHRROMSize}\n\n" +

                $"PRGRAMs: {header.PRGRAMSize}\n" +
                $"PRGRAMBs: {header.PRGRAMWithBatterySize}\n\n" +

                $"CHRRAMs: {header.CHRRAMSize}\n" +
                $"CHRRAMBs: {header.CHRRAMWithBatterySize}\n\n" +

                $"HasTrainer: {header.HasTrainer}\n\n" +

                $"Mapper: {header.MapperNumber}\n" +
                $"SubMapper: {header.SubmapperNumber}\n\n" +

                $"Mirroring: {header.Mirroring}\n" +
                $"TvSystem: {header.TvSystem}\n\n" +

                $"Bus Conflicts: {header.HasBusConflicts}"
                );
        }
    }
}