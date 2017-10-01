using System;

namespace ASD.NESCore {

    using Common;
    using Helpers;
    using CartridgeParts;

    public sealed class Cartridge {

        private MemoryBus memory = MemoryBus.Instance;

        private byte[] data;

        private Cartridge() { }

        public static Cartridge Create(byte[] data) {

            var header = new Header(data);

            throw new Information($"Rom is Valid: {header.IsValid}");
        }
    }
}