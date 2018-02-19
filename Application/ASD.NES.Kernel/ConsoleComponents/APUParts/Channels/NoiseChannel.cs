namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Channels {

    using BasicComponents;
    using Helpers;
    using Registers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification Noise ($400C-400F)
    // http://wiki.nesdev.com/w/index.php/APU_Noise
    internal sealed class NoiseChannel : AudioChannel {

        // register[0] - $400C : --LC VVVV : Length counter halt (L), Constant volume (C), Volume/Envelope (V)
        public bool LengthCounterDisabled => r[0].HasBit(5);
        public bool EnvelopeDecayDisabled => r[0].HasBit(4);
        public byte Volume => r[0].L();

        // register[1] - $400D : ---- ---- : isn't used?

        // register[2] - $400E : M--- PPPP : Mode flag (M), Timer period index from table
        public bool ModeFlagIsSet => r[2].HasBit(7);
        public byte Period => r[2].L();

        // register[3] - $400F : LLLL L--- : Length counter load and envelope restart
        public byte LengthCounterLoad => (byte)(r[3] >> 3);

        // ------- Additional -------

        public bool EnvelopeLoop => LengthCounterDisabled;

        // flag is shared with LengthCounterHalt
        public byte EnvelopeVolume { get; set; }
        public int EnvelopeCounter { get; set; }
        public int ShiftRegister { get; set; } = 1;
        public int LengthCounter { get; set; }

        // http://wiki.nesdev.com/w/index.php/APU_Noise // NTSC
        private int[] NoiseFrequencyTable { get; } = {
            4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068
        };

        public NoiseChannel(AudioChannelRegisters registers, int clockSpeed, int sampleRate)
            : base(registers, clockSpeed, sampleRate) { }

        // The length counter provides automatic duration control for the NES APU waveform channels.
        // http://wiki.nesdev.com/w/index.php/APU_Length_Counter
        public override void TickLengthCounter() { // TODO: clarify, bug, long-time
            if (!LengthCounterDisabled) {
                LengthCounter -= 1;
                if (LengthCounter < 0) {
                    LengthCounter = 0;
                }
            }
        }        

        public void TickEnvelopeCounter() {
            if (EnvelopeCounter == 0) {
                if (EnvelopeVolume == 0) {
                    if (EnvelopeLoop) {
                        EnvelopeVolume = 15;
                    }
                }
                else {
                    EnvelopeVolume--;
                }
                EnvelopeCounter = Volume;
            }
            else {
                EnvelopeCounter--;
            }
        }

        public void TickShiftRegister() {
            var shifter = ModeFlagIsSet ? 8 : 13;
            ShiftRegister = (ShiftRegister << 1) | ((ShiftRegister >> 14 ^ ShiftRegister >> shifter) & 0x1);
        }

        // TODO: clarify
        public override float GetAudio() {

            SampleCount++;
            if (SampleCount >= RenderedWaveLength) {
                SampleCount -= RenderedWaveLength / 16;
                TickShiftRegister();
            }

            var regBit = ShiftRegister & 1;
            var period = (-1 + (regBit == 1 ? regBit <<= 1 : regBit));
            var volume = (EnvelopeDecayDisabled ? Volume : EnvelopeVolume) / 15.0f;

            Timer = NoiseFrequencyTable[Period];
            UpdateFrequency();

            return period * volume;
        }

        // TODO: clarify
        public override void OnRegisterChanged(int address) {
            switch (address & 0b11) {
                case 0b00:
                    EnvelopeVolume = 15;
                    EnvelopeCounter = Volume;
                    break;
                case 0b11:
                    LengthCounter = waveLengths[LengthCounterLoad];
                    break;
            }
        }
    }
}