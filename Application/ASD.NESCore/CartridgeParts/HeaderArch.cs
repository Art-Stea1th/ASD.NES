namespace ASD.NESCore.CartridgeParts {

    using Helpers;

    internal class HeaderArch : Header {

        public override DataFormat DataFormat => DataFormat.ArchNES;
        public HeaderArch(byte[] header) : base(header) { }

        public override int PRGROMSize => header[4] * 0x4000;
        public override int CHRROMSize => header[5] * 0x2000;

        public override int PRGRAMSize => header[6].HasBit(1) || header[10].HasBit(4) ? 0x2000 : 0;
        public override int CHRRAMSize => CHRROMSize == 0 ? 0x2000 : 0;

        public override bool HasTrainer => header[6].HasBit(2); // 512-byte at $7000-$71FF (stored before PRG data)

        public override Mirroring Mirroring =>
            header[6].HasBit(3) ? Mirroring.FourScreen
            : header[6].HasBit(0) ? Mirroring.Vertical
            : Mirroring.Horizontal;

        public override int MapperNumber => header[6].HNybble();

        public override TvSystem TvSystem =>
            header[10].HasBit(0) ? TvSystem.DualCompatible
            : header[10].HasBit(1) ? TvSystem.PAL
            : TvSystem.NTSC;

        public override bool HasBusConflicts => header[10].HasBit(5);
    }
}