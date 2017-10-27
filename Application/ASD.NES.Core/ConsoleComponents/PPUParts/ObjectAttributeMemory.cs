namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    using BasicComponents;
    using Shared;
    using static ObjectAttributeMemory;

    internal sealed class ObjectAttributeMemory : IMemory<OAMRecord> {

        private OAMRecord[] record = new OAMRecord[64];
        public OAMRecord this[int address] {
            get => record[address];
            set => record[address] = value;
        }

        public int Cells => record.Length;

        internal struct OAMRecord {

            private Quadlet record;

            public Octet Y => (Octet)(record.L.L + 1);
            public Octet TileNumber => record.L.H;

            public Octet ColorBitsH => (Octet)(record.H.L & 0b11);
            public bool InFront => !record.H.L[5];
            public bool FlipH => record.H.L[6];
            public bool FlipV => record.H.L[7];

            public Octet X => record.H.H;

            public static implicit operator OAMRecord(Quadlet record) => new OAMRecord(record);
            public OAMRecord(Quadlet record) => this.record = record;
        }
    }
}