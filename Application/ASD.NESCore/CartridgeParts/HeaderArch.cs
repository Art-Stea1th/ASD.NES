namespace ASD.NESCore.CartridgeParts {

    using Helpers;

    internal class HeaderArch : Header {

        public override DataFormat DataFormat => DataFormat.ArchNES;
        public HeaderArch(byte[] header) : base(header) { }

        public override int PRGROMs => header[4];
        public override int CHRROMs => header[5];

        public override int PRGRAMs => header[6].HasBit(1) || header[10].HasBit(4) ? 1 : 0;
        public override int CHRRAMs => CHRROMs == 0 ? 1 : 0;

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