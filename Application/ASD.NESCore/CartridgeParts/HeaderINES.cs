namespace ASD.NESCore.CartridgeParts {

    using Helpers;

    internal class HeaderINES : HeaderArch {

        public override DataFormat DataFormat => DataFormat.INES;
        public HeaderINES(byte[] header) : base(header) { }

        public override bool HasVSUnisystem => header[7].HasBit(0);
        public override int HasPlayChoice => header[7].HasBit(1) ? 1 : 0;

        public override int PRGRAMs => header[8] == 0 ? 1 : header[8];

        public override int MapperNumber => BitOperations.MakeInt8(header[7].HNybble(), (byte)base.MapperNumber);
        public override TvSystem TvSystem => header[9].HasBit(0) ? TvSystem.PAL : TvSystem.NTSC;
    }
}