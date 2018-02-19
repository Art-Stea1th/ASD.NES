namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Channels {

    using Helpers;
    using Registers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification
    // http://wiki.nesdev.com/w/index.php/APU_Noise
    internal sealed class NoiseChannel : AudioChannel {

        // register[0] - $400C : --LC VVVV : Length counter halt (L), Constant volume (C), Volume/Envelope (V)
        protected override bool LengthCounterDisabled => r[0].HasBit(5);
        protected override bool EnvelopeDecayDisabled => r[0].HasBit(4);
        protected override byte Volume => r[0].L();

        // register[1] - $400D : ---- ---- : isn't used?

        // register[2] - $400E : M--- PPPP : Mode flag (M), Timer period index from table
        public bool ModeFlagIsSet => r[2].HasBit(7);
        public byte FrequencyTableIndex => r[2].L();

        // register[3] - $400F : LLLL L--- : Length counter load and envelope restart
        protected override byte LengthIndex => (byte)(r[3] >> 3);
        public int ShiftRegister { get; set; } = 1;

        // http://wiki.nesdev.com/w/index.php/APU_Noise // NTSC
        private int[] NoiseFrequencyTable { get; } = {
            4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068
        };

        public NoiseChannel(AudioChannelRegisters registers, int clockSpeed, int sampleRate)
            : base(registers, clockSpeed, sampleRate) { }

        public void TickShiftRegister() {
            var shifter = ModeFlagIsSet ? 8 : 13;
            ShiftRegister = (ShiftRegister << 1) | ((ShiftRegister >> 14 ^ ShiftRegister >> shifter) & 0x1);
        }

        public override float GetAudio() {

            Timer = NoiseFrequencyTable[FrequencyTableIndex];
            UpdateFrequency();

            SampleCount++;
            if (SampleCount >= RenderedWaveLength) {
                SampleCount -= RenderedWaveLength;
                TickShiftRegister();
            }
            var randomBit = ShiftRegister & 1;

            var period = (-1 + (randomBit == 1 ? randomBit <<= 1 : randomBit));
            var volume = (EnvelopeDecayDisabled ? Volume : EnvelopeVolume) / 15.0f;

            return period * volume;
        }

        public override void OnRegisterChanged(int address) {
            switch (address & 0b11) {
                case 0b00:
                    EnvelopeCounter = Volume;
                    EnvelopeCounter++;
                    break;
                case 0b11:
                    LengthCounter = waveLengths[LengthIndex];
                    if (!EnvelopeDecayDisabled) { EnvelopeVolume = 15; }
                    break;
            }
        }
    }
}