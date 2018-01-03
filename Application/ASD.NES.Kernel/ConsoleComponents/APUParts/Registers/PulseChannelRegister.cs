namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Registers {

    using BasicComponents;
    using Shared;

    // http://wiki.nesdev.com/w/index.php/APU#Specification Pulse ($4000-4007)
    // http://wiki.nesdev.com/w/index.php/APU_Pulse
    internal sealed class PulseChannelRegister : IMemory<Octet> { // byte x 4 / 32bit

        private Octet[] quadlet = new Octet[4];
        public Octet this[int address] {
            get => quadlet[address & 3];
            set => quadlet[address & 3] = value;
        }
        public int Cells => quadlet.Length;

        // quadlet[0] - $4000 | $4004 : DDLC VVVV : Duty (D), envelope loop / length counter halt (L), constant volume (C), volume/envelope (V)
        public byte Duty => (byte)(quadlet[0] >> 6 & 3);
        public bool LengthCounterHalt => quadlet[0][5];
        public bool ConstantVolume => quadlet[0][4];
        public byte EnvelopeDividerPeriodOrVolume => (byte)(quadlet[0] & 0b1111);

        // quadlet[1] - $4001 | $4005 : EPPP NSSS : Sweep unit: enabled (E), period (P), negate (N), shift (S)
        public bool SweepEnabled => quadlet[1][7];
        public byte SweepPeriod => (byte)((quadlet[1] >> 4) & 0b111);
        public bool SweepNegate => quadlet[1][3];
        public byte SweepShift => (byte)(quadlet[1] & 0b111);

        // quadlet[2] - $4002 | $4006 : TTTT TTTT : Timer low (T)
        // quadlet[3] - $4003 | $4007 : LLLL LTTT : Length counter load (L), timer high (T)

        public ushort Timer {
            get => (ushort)(((quadlet[3] & 0b111) << 8) | quadlet[2]);
            set {
                quadlet[2] = (byte)value;
                quadlet[3] = (byte)((quadlet[3] | 0b1111_1000) | ((value >> 8) & 0b111));
            }
        }
        public byte LengthCounterLoad => (byte)(quadlet[3] >> 3);
    }
}