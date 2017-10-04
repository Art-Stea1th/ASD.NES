using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD.NESCore {

    using CartridgeParts;
    using Common;
    using Helpers;



    public sealed class Cartridge {

        internal IReadOnlyCollection<MemoryBank> PRGROM { get; private set; }
        internal IReadOnlyCollection<MemoryBank> CHRROM { get; private set; }

        internal IReadOnlyCollection<MemoryBank> PRGRAM { get; private set; }
        internal IReadOnlyCollection<MemoryBank> CHRRAM { get; private set; }

        public static Cartridge Create(byte[] data) {
            return new Cartridge(Header.Create(data), data.Skip(16).ToArray());
        }

        private Cartridge(Header header, byte[] data) {
            switch (header) {
                case HeaderNES2 h: FillData(h, data); break;
                case HeaderINES h: FillData(h, data); break;
                case HeaderArch h: FillData(h, data); break;
            }
        }

        private void FillData(HeaderNES2 header, byte[] data) {
            throw IsNotYetSupported("NES 2.0 files format");
        }

        private void FillData(HeaderINES header, byte[] data) {

        }

        private void FillData(HeaderArch header, byte[] data) {
            throw IsNotYetSupported("Archaic iNES files format");
        }        

        private Exception IsNotYetSupported(string target)
            => new Information($"{target} is not yet supported.");


        

    }
}