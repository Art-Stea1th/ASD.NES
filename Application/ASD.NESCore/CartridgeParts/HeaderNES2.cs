namespace ASD.NESCore.CartridgeParts {

    using Helpers;

    internal sealed class HeaderNES2 : HeaderINES {

        public override DataFormat DataFormat => DataFormat.NES20;
        public HeaderNES2(byte[] header) : base(header) { }

        public override int MapperNumber => BitOperations.MakeInt16(header[8].HNybble(), (byte)base.MapperNumber);
        public override int SubmapperNumber => header[8].LNybble();

        public override int PRGROMSize => BitOperations.MakeInt16(header[9].LNybble(), (byte)base.PRGROMSize);
        public override int CHRROMSize => BitOperations.MakeInt16(header[9].HNybble(), (byte)base.CHRROMSize);

        public override int PRGRAMSize => header[10].LNybble() > 0 ? 1 << (header[10].LNybble() + 6) : 0;
        public override int PRGRAMWithBatterySize => header[10].HNybble() > 0 ? 1 << (header[10].HNybble() + 6) : 0;

        public override int CHRRAMSize => header[11].LNybble() > 0 ? 1 << (header[11].LNybble() + 6) : 0;
        public override int CHRRAMWithBatterySize => header[11].HNybble() > 0 ? 1 << (header[11].HNybble() + 6) : 0;

        public override TvSystem TvSystem =>
            header[12].HasBit(1) ? TvSystem.DualCompatible
            : header[12].HasBit(0) ? TvSystem.PAL
            : TvSystem.NTSC;
    }
}