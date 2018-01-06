namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Registers {

    // TODO: Separate channel & registers

    using BasicComponents;
    using Shared;

    internal sealed class TriangleChannel : IMemory<Octet> {

        private Octet[] quadlet = new Octet[4];
        public Octet this[int address] {
            get => quadlet[address & 3];
            set => quadlet[address & 3] = value;
        }
        public int Cells => quadlet.Length;

        // ------- Registers -------

        // quadlet[0] - $4008 : CRRR RRRR : Length counter halt / linear counter control(C), linear counter load(R)
        public bool LengthCounterHalt => quadlet[0][7];
        public byte LinearCounterLoad {
            get => (byte)(quadlet[0] & 0x7F);
            set => quadlet[0] = (byte)((quadlet[0] & 0x80) | (value & 0x7F));
        }

        // quadlet[1] - $4009 : ---- ---- : Unused
        // quadlet[2] - $400A : TTTT TTTT : Timer low(T)
        // quadlet[3] - $400B : LLLL LTTT : Length counter load(L), timer high(T)

        public ushort Timer {
            get => (ushort)(((quadlet[3] & 0b111) << 8) | quadlet[2]);
            set {
                quadlet[2] = (byte)value;
                quadlet[3] = (byte)((quadlet[3] | 0b1111_1000) | ((value >> 8) & 0b111));
            }
        }

        public byte LengthCounterLoad => (byte)(quadlet[3] >> 3);


        // ------- Additional -------

        public int CurrentLengthCounter { get; set; }
        public int CurrentLinearCounter { get; set; }

        public void TickLengthCounter() {
            if (!LengthCounterHalt) {
                CurrentLengthCounter -= 1;
                if (CurrentLengthCounter < 0) {
                    CurrentLengthCounter = 0;
                }
            }
        }

        public void TickLinearCounter() {
            if (!LengthCounterHalt) {
                if (LinearCounterLoad == 0) {
                    LinearCounterLoad = 0;
                }
                else {
                    LinearCounterLoad -= 1;
                }
            }
        }

        public float GetTriangleAudio(int timeInSamples, int sampleRate) {

            // One octave lower than pulse frequency
            var frequency = 111860.0 / Timer / 2;
            var normalizedSampleTime = timeInSamples * frequency / sampleRate;

            var normalized = ((timeInSamples * (int)frequency) % sampleRate) / (float)sampleRate;

            // Map [0,1) to the triangle in range [-1,1]
            if (normalized <= 0.5) {
                return -1f + 4f * normalized;
            }
            else {
                return 3f - 4f * normalized;
            }
        }
    }
}