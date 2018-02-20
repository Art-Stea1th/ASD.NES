namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    using BasicComponents;
    using Helpers;
    using static ObjectAttributeMemory;

    internal sealed class ObjectAttributeMemory : IMemory<OAMRecord> {

        private OAMRecord[] record = new OAMRecord[64];
        public OAMRecord this[int address] {
            get => record[address];
            set => record[address] = value;
        }

        public int Cells => record.Length;

        internal struct OAMRecord {

            private uint record;

            public byte Y => record.LL();
            public byte TileNumber => record.LH();

            public byte ColorBitsH => (byte)(record.HL() & 0b11);
            public bool InFront => !record.HL().HasBit(5);
            public bool FlipH => record.HL().HasBit(6);
            public bool FlipV => record.HL().HasBit(7);

            public byte X => record.HH();

            public static implicit operator OAMRecord(uint record) => new OAMRecord(record + 1); // record.Y + 1
            public OAMRecord(uint record) => this.record = record;
        }
    }
}