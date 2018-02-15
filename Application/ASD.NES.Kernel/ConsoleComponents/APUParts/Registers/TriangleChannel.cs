namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Registers {

    // TODO: Separate channels & registers, Add 'base-classes' for them

    using BasicComponents;
    using Helpers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification Triangle ($4008-400B)
    // http://wiki.nesdev.com/w/index.php/APU_Triangle
    internal sealed class TriangleChannel : IMemory<byte> {

        // ------- Registers -------

        private byte[] r = new byte[4];
        public byte this[int address] {
            get => r[address & 3];
            set => r[address & 3] = value;
        }
        public int Cells => r.Length;

        // register[0] - $4008 : CRRR RRRR : Length counter halt / linear counter control(C), linear counter load(R)
        public bool LengthCounterHalt => r[0].HasBit(7);
        public byte LinearCounterLoad {
            get => (byte)(r[0] & 0x7F);
            set => r[0] = (byte)((r[0] & 0x80) | (value & 0x7F));
        }

        // register[1] - $4009 : ---- ---- : Unused
        // register[2] - $400A : TTTT TTTT : Timer low(T)
        // register[3] - $400B : LLLL LTTT : Length counter load(L), timer high(T)

        public ushort Timer {
            get => (ushort)(((r[3] & 0b111) << 8) | r[2]);
            set {
                r[2] = (byte)value;
                r[3] = (byte)((r[3] | 0b1111_1000) | ((value >> 8) & 0b111));
            }
        }

        public byte LengthCounterLoad => (byte)(r[3] >> 3);


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