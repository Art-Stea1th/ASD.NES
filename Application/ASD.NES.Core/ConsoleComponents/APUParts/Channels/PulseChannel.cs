namespace ASD.NES.Core.ConsoleComponents.APUParts.Channels {

    using Helpers;
    using Registers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification
    // http://wiki.nesdev.com/w/index.php/APU_Pulse
    internal sealed class PulseChannel : AudioChannel {

        // register[0] - $4000 | $4004 : DDLC VVVV : Duty (D), length counter disable (L), constant volume / envelope decay disable (C), volume/envelope (V)
        private byte DutyMapIndex => (byte)(r[0] >> 6 & 0b11);
        protected override bool LengthCounterDisabled => r[0].HasBit(5);
        protected override bool EnvelopeDecayDisabled => r[0].HasBit(4);
        protected override byte Volume => r[0].L();

        // register[1] - $4001 | $4005 : EPPP NSSS : Sweep unit: enabled (E), period (P), negate (N), shift (S)
        private bool SweepEnabled { get => r[1].HasBit(7); set => r[1] = r[1].WithChangedBit(7, value); }
        private byte SweepPeriod => (byte)((r[1] >> 4) & 0b111);
        private bool SweepNegate => r[1].HasBit(3);
        private byte SweepShift => (byte)(r[1] & 0b111);

        // register[2] - $4002 | $4006 : TTTT TTTT : Timer low (T)
        // register[3] - $4003 | $4007 : LLLL LTTT : Length counter load (L), timer high (T)

        protected override int Timer {
            get => ((r[3] & 0b111) << 8) | r[2];
            set { r[2] = (byte)value; r[3] = (byte)((r[3] | 0b1111_1000) | ((value >> 8) & 0b111)); }
        }
        protected override byte LengthIndex => (byte)(r[3] >> 3);
        public int SweepCounter { get; set; }

        private static float[] DutyMap { get; set; } = new float[] { .125f, .25f, .5f, .75f };

        public PulseChannel(AudioChannelRegisters registers, int clockSpeed, int sampleRate)
            : base(registers, clockSpeed, sampleRate) { }

        private bool squarePositive = true;
        public override float GetAudio() {

            UpdateFrequency();

            var waveLength = default(double);
            var period = default(float);

            if (squarePositive) {
                waveLength = 16 * RenderedWaveLength * DutyMap[DutyMapIndex];
                period = 1.0f;
            }
            else {
                waveLength = 16 * RenderedWaveLength * (1.0 - DutyMap[DutyMapIndex]);
                period = -1.0f;
            }

            SampleCount++;
            if (SampleCount >= waveLength) {
                SampleCount -= waveLength;
                squarePositive = !squarePositive;
            }

            var volume = (EnvelopeDecayDisabled ? Volume : EnvelopeVolume) / 15.0f;

            return period * volume;
        }

        // http://wiki.nesdev.com/w/index.php/APU_Sweep
        public override void TickSweep() {

            if (SweepCounter == 0) {
                if (SweepEnabled) {
                    var periodAdjustment = Timer >> SweepShift;
                    int newPeriod;
                    if (SweepNegate) {
                        newPeriod = Timer - periodAdjustment;
                    }
                    else {
                        newPeriod = Timer + periodAdjustment;
                    }
                    if (newPeriod > 0 && newPeriod < 0x7FF && SweepShift != 0) {
                        Timer = (ushort)newPeriod;
                    }
                }
                SweepCounter = SweepPeriod;
            }
            else {
                SweepCounter--;
            }
        }        

        public override void OnRegisterChanged(int address) {
            switch (address & 0b11) {
                case 0b00:
                    EnvelopeCounter = Volume;
                    EnvelopeCounter++;
                    break;
                case 0b01:
                    SweepCounter = SweepPeriod;
                    break;
                case 0b11:
                    LengthCounter = waveLengths[LengthIndex];
                    if (!EnvelopeDecayDisabled) { EnvelopeVolume = 15; }
                    break;
            }
        }
    }
}