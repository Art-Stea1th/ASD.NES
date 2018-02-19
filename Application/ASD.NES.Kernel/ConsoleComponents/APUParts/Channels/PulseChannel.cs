using System;

namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Channels {

    using Helpers;
    using Registers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification Pulse ($4000-4007)
    // http://wiki.nesdev.com/w/index.php/APU_Pulse
    internal sealed class PulseChannel : AudioChannel {

        // register[0] - $4000 | $4004 : DDLC VVVV : Duty (D), length counter disable (L), constant volume / envelope decay disable (C), volume/envelope (V)
        public byte Duty => (byte)(r[0] >> 6 & 0b11);
        public bool LengthCounterDisable => r[0].HasBit(5);
        public bool EnvelopeDecayDisable => r[0].HasBit(4);
        public byte Volume => r[0].L();

        // register[1] - $4001 | $4005 : EPPP NSSS : Sweep unit: enabled (E), period (P), negate (N), shift (S)
        public bool SweepEnabled { get => r[1].HasBit(7); set => r[1] = r[1].WithChangedBit(7, value); }
        public byte SweepPeriod => (byte)((r[1] >> 4) & 0b111);
        public bool SweepNegate => r[1].HasBit(3);
        public byte SweepShift => (byte)(r[1] & 0b111);

        // register[2] - $4002 | $4006 : TTTT TTTT : Timer low (T)
        // register[3] - $4003 | $4007 : LLLL LTTT : Length counter load (L), timer high (T)

        protected override int Timer {
            get => ((r[3] & 0b111) << 8) | r[2];
            set { r[2] = (byte)value; r[3] = (byte)((r[3] | 0b1111_1000) | ((value >> 8) & 0b111)); }
        }
        public byte LengthCounterLoad => (byte)(r[3] >> 3);

        // ------- Additional -------

        public bool EnvelopeLoop => LengthCounterDisable;

        // flag is shared with LengthCounterHalt
        public byte EnvelopeVolume { get; set; }
        public int EnvelopeCounter { get; set; }
        public int SweepCounter { get; set; }
        public int LengthCounter { get; set; }

        // http://wiki.nesdev.com/w/index.php/APU_Pulse 
        private static float[] DutyMap { get; set; } = new float[] { .125f, .25f, .5f, .75f };

        public PulseChannel(AudioChannelRegisters registers, int clockSpeed, int sampleRate)
            : base(registers, clockSpeed, sampleRate) { }

        // The length counter provides automatic duration control for the NES APU waveform channels.
        // http://wiki.nesdev.com/w/index.php/APU_Length_Counter
        public override void TickLengthCounter() {
            if (!LengthCounterDisable) {
                LengthCounter -= 1;
                if (LengthCounter < 0) {
                    LengthCounter = 0;
                }
            }
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

        // APU Envelope http://wiki.nesdev.com/w/index.php/APU_Envelope
        // ADSR Envelope https://en.wikipedia.org/wiki/Synthesizer#ADSR_envelope
        // https://ru.wikipedia.org/wiki/ADSR-%D0%BE%D0%B3%D0%B8%D0%B1%D0%B0%D1%8E%D1%89%D0%B0%D1%8F
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

        protected override void UpdateFrequency() {
            Frequency = 111860.0f / Timer;
        }

        public override float GetAudio() {

            SampleCount++;
            if (SampleCount < 0) { SampleCount = 0; }
            UpdateFrequency();

            var normalizedSampleTime = SampleCount * Frequency / SampleRate;

            var fractionalNormalizedSampleTime = normalizedSampleTime - Math.Floor(normalizedSampleTime); // 0 ... 0.999
            var dutyPulse = fractionalNormalizedSampleTime < DutyMap[Duty] ? 1f : -1f;

            var volume = EnvelopeVolume;
            if (EnvelopeDecayDisable) {
                volume = Volume;
            }
            return dutyPulse * (volume / 15.0f);
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
                    LengthCounter = waveLengths[LengthCounterLoad];
                    if (!EnvelopeDecayDisable) { EnvelopeVolume = 15; }
                    break;
            }
            
        }
    }
}