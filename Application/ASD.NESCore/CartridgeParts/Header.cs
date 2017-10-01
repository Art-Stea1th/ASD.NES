using System.Linq;

namespace ASD.NESCore.CartridgeParts {

    internal enum FileType { ArchNES, INES, NES20, Unknown }

    internal sealed class Header {

        private byte[] header;

        public bool IsValid => ContainsNESLabel();
        public int PRGROMPagesCount => header[4];
        public int CHRROMPagesCount => header[5];

        public Header(byte[] data) => header = data.Take(16).ToArray();

        private bool ContainsNESLabel()
            => new string(header.Take(3).Select(c => (char)c).ToArray()) == "NES";
    }
}