namespace ASD.NESCore.CartridgeParts {

    using Helpers;

    internal sealed class HeaderNES2 : HeaderINES {

        public override DataFormat DataFormat => DataFormat.NES20;
        public HeaderNES2(byte[] header) : base(header) { }

        public override int MapperNumber => BitOperations.MakeInt16(header[8].HNybble(), (byte)base.MapperNumber);
        public override int SubmapperNumber => header[8].LNybble();

        public override int PRGROMs => BitOperations.MakeInt16(header[9].LNybble(), (byte)base.PRGROMs);
        public override int CHRROMs => BitOperations.MakeInt16(header[9].HNybble(), (byte)base.CHRROMs);

        public override int PRGRAMs => header[10].LNybble();
        public override int PRGRAMsWithBattery => header[10].HNybble();

        public override int CHRRAMs => header[11].LNybble();
        public override int CHRRAMsWithBattery => header[11].HNybble();

        public override TvSystem TvSystem =>
            header[12].HasBit(1) ? TvSystem.DualCompatible
            : header[12].HasBit(0) ? TvSystem.PAL
            : TvSystem.NTSC;
    }
}