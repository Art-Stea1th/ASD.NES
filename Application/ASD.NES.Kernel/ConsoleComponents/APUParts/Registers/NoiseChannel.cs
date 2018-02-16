namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Registers {
    using System;

    // TODO: Separate channels & registers, Add 'base-classes' for them

    using BasicComponents;
    using Helpers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification Noise ($400C-400F)
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
        public bool LengthCounterHalt => r[0].HasBit(5);
        public bool ConstantVolume => r[0].HasBit(4);
        public byte EnvelopeDividerPeriodOrVolume => r[0].L();

        // register[1] - $400D : ---- ---- : isn't used?

        // register[2] - $400E : M--- PPPP : Mode flag (M), Timer period ndx from table
        public bool ModeFlagIsSet => r[2].HasBit(7);
        public byte Period => r[2].L();

        // register[3] - $400F : LLLL L--- : Length counter load and envelope restart
        public byte LengthCounterLoad => (byte)(r[3] >> 3);

        // ------- Additional -------

        public bool EnvelopeLoop => LengthCounterHalt;

        // flag is shared with LengthCounterHalt
        public byte EnvelopeVolume { get; set; }
        public int EnvelopeCounter { get; set; }
        public int ShiftRegister { get; set; } = 1;
        public int CurrentLengthCounter { get; set; }

        // http://wiki.nesdev.com/w/index.php/APU_Noise // NTSC
        public int[] NoiseFrequencyTable { get; } = {
            4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068
        };


        // The length counter provides automatic duration control for the NES APU waveform channels.
        // http://wiki.nesdev.com/w/index.php/APU_Length_Counter
        public void TickLengthCounter() {
            if (!LengthCounterHalt) {
                CurrentLengthCounter -= 1;
                if (CurrentLengthCounter < 0) {
                    CurrentLengthCounter = 0;
                }
            }
        }

        public void TickShiftRegister() {

            var shifter = ModeFlagIsSet ? 8 : 13;

            ShiftRegister = (ShiftRegister << 1) | ((ShiftRegister >> 14 ^ ShiftRegister >> shifter) & 0x1);

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
                EnvelopeCounter = EnvelopeDividerPeriodOrVolume;
            }
            else {
                EnvelopeCounter--;
            }
        }

        // TODO: impl. pitch for this
        public float GetNoiseAudio(int timeInSamples, int sampleRate) {

            TickShiftRegister();

            var volume = EnvelopeVolume;
            if (ConstantVolume) {
                volume = EnvelopeDividerPeriodOrVolume;
            }

            return (ShiftRegister & 0b1) * (volume / 2);
        }
    }
}