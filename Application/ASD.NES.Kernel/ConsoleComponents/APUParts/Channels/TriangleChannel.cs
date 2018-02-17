namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Channels {

    using BasicComponents;
    using Helpers;
    using Registers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification Triangle ($4008-400B)
    // http://wiki.nesdev.com/w/index.php/APU_Triangle
    internal sealed class TriangleChannel : AudioChannel {

        // ------- Registers -------

        private TriangleChannelRegisters r;
        public override IMemory<byte> Registers => r;

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


        public TriangleChannel(TriangleChannelRegisters registers) {
            r = registers;
            r.Changed += OnRegisterChanged;
        }

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

        public float GetAudio(int timeInSamples, int sampleRate) {

            // One octave lower than pulse frequency
            var frequency = 111860.0 / Timer / 2;
            var normalizedSampleTime = ((timeInSamples * (int)frequency) % sampleRate) / (float)sampleRate;

            if (normalizedSampleTime <= 0.5) {
                return -1f + 4f * normalizedSampleTime;
            }
            else {
                return 3f - 4f * normalizedSampleTime;
            }
        }

        public override float GetAudio() {
            throw new System.NotImplementedException();
        }

        public override void OnRegisterChanged(int address) {
            switch (address & 0b11) {
                case 0b00:
                    CurrentLinearCounter = LinearCounterLoad;
                    break;
                case 0b11:
                    CurrentLengthCounter = lengthCounterLookupTable[LengthCounterLoad];
                    CurrentLinearCounter = LinearCounterLoad;
                    break;
            }
        }
    }
}