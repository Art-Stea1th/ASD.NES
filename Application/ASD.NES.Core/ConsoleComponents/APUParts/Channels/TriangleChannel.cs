namespace ASD.NES.Core.ConsoleComponents.APUParts.Channels {

    using Helpers;
    using Registers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification
    // http://wiki.nesdev.com/w/index.php/APU_Triangle
    internal sealed class TriangleChannel : AudioChannel {

        // register[0] - $4008 : CRRR RRRR : Length counter halt / linear counter control(C), linear counter load(R)
        protected override bool LengthCounterDisabled => r[0].HasBit(7);
        private byte LinearCounterLoad {
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
        public int LinearCounter { get; private set; }

        float[] triangleForm = {
            0, +2, +4, +6, +8, +10, +12, +14, +15, +14, +12, +10, +8, +6, +4, +2,
            0, -2, -4, -6, -8, -10, -12, -14, -15, -14, -12, -10, -8, -6, -4, -2
        };

        public TriangleChannel(AudioChannelRegisters registers, int clockSpeed, int sampleRate)
            : base(registers, clockSpeed, sampleRate) { }        

        uint tick = 0;
        public override float GetAudio() {

            UpdateFrequency();

            if (Timer > 0) {
                SampleCount++;
                if (SampleCount >= RenderedWaveLength) {
                    SampleCount -= RenderedWaveLength;
                    tick++;
                }
                return triangleForm[tick & 0x1F] / 15.0f;
            }
            return 0f;
        }

        public void TickLinearCounter() {
            if (!LengthCounterDisabled) {
                if (LinearCounterLoad == 0) {
                    LinearCounterLoad = 0;
                }
                else {
                    LinearCounterLoad--;
                }
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