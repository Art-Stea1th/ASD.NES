namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Registers {

    // TODO: Separate channels & registers, Add 'base-classes' for them

    using BasicComponents;
    using Helpers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification Pulse ($400C-400F)
    // http://wiki.nesdev.com/w/index.php/APU_Noise
    internal sealed class NoiseChannel : IMemory<byte> {

        // ------- Registers -------

        private byte[] r = new byte[4];
        public byte this[int address] {
            get => r[address & 3];
            set => r[address & 3] = value;
        }
        public int Cells => r.Length;

        // register[0] - $400C : --LC VVVV : length counter halt (L), constant volume (C), volume/envelope (V)
        // !! WRITE for all or envelope only?
        public bool LengthCounterHalt => r[0].HasBit(5);
        public bool ConstantVolume => r[0].HasBit(4);
        public byte EnvelopeDividerPeriodOrVolume => (byte)(r[0] & 0b1111);

        // register[1] - $400D : ---- ---- : isn't used?

        // register[2] - $400E : M--- PPPP : !! WRITE ? Mode flag (M), Timer period ndx from table
        public byte Mode => (byte)(r[2] >> 7);
        public byte Period => (byte)(r[2] & 0b1111);

        // register[3] - $400F : LLLL L--- : !! WRITE ? Length counter load and envelope restart
        public byte LengthCounterLoad => (byte)(r[3] >> 3);
    }
}