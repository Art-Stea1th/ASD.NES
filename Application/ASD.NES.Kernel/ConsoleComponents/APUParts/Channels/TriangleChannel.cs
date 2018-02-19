namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Channels {

    using Helpers;
    using Registers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification Triangle ($4008-400B)
    // http://wiki.nesdev.com/w/index.php/APU_Triangle
    internal sealed class TriangleChannel : AudioChannel {

        // register[0] - $4008 : CRRR RRRR : Length counter halt / linear counter control(C), linear counter load(R)
        public bool LengthCounterHalt => r[0].HasBit(7);
        public byte LinearCounterLoad {
            get => (byte)(r[0] & 0x7F);
            set => r[0] = (byte)((r[0] & 0x80) | (value & 0x7F));
        }

        // register[1] - $4009 : ---- ---- : Unused
        // register[2] - $400A : TTTT TTTT : Timer low(T)
        // register[3] - $400B : LLLL LTTT : Length counter load(L), timer high(T)

        protected override int Timer {
            get => ((r[3] & 0b111) << 8) | r[2];
            set {
                r[2] = (byte)value;
                r[3] = (byte)((r[3] | 0b1111_1000) | ((value >> 8) & 0b111));
            }
        }

        protected override byte LengthIndex => (byte)(r[3] >> 3);


        // ------- Additional -------

        public int LinearCounter { get; set; }

        public TriangleChannel(AudioChannelRegisters registers, int clockSpeed, int sampleRate)
            : base(registers, clockSpeed, sampleRate) { }

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

        protected override void UpdateFrequency() {
            // One octave lower than pulse frequency
            Frequency = 111860.0 / Timer / 2;
        }

        public override float GetAudio() {

            SampleCount++;
            if (SampleCount < 0) { SampleCount = 0; }
            UpdateFrequency();

            var normalizedSampleTime = ((int)(SampleCount * Frequency) % (int)SampleRate) / (float)SampleRate;

            if (normalizedSampleTime <= 0.5) {
                return -1f + 4f * normalizedSampleTime;
            }
            else {
                return 3f - 4f * normalizedSampleTime;
            }
        }

        public override void OnRegisterChanged(int address) {
            switch (address & 0b11) {
                case 0b00:
                    LinearCounter = LinearCounterLoad;
                    break;
                case 0b10:
                    break;
                case 0b11:
                    LengthCounter = waveLengths[LengthIndex];
                    LinearCounter = LinearCounterLoad;
                    break;
            }
        }
    }
}