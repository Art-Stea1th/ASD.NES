namespace ASD.NESCore.CartridgeParts {

    using Helpers;

    internal class HeaderINES : HeaderArch {

        public override DataFormat DataFormat => DataFormat.INES;
        public HeaderINES(byte[] header) : base(header) { }

        public override bool IsVSUnisystem => header[7].HasBit(0);
        public override int INSTROMSize => header[7].HasBit(1) ? 0x2000 : 0;

        public override int PRGRAMSize => header[8] == 0 ? 0x2000 : header[8] * 0x2000;

        public override int MapperNumber => BitOperations.MakeInt8(header[7].HNybble(), (byte)base.MapperNumber);
        public override TvSystem TvSystem => header[9].HasBit(0) ? TvSystem.PAL : TvSystem.NTSC;
    }
}