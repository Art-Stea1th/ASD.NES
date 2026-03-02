namespace ASD.NES.Core {

    /// <summary> CPU register state for tests / debug (6502 spec). </summary>
    internal struct CpuState {

        public byte A;
        public byte X;
        public byte Y;
        public byte SP;
        public ushort PC;
        public byte P;
    }
}
