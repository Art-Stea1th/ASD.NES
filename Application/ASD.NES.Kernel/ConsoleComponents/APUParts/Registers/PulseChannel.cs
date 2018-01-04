namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Registers {

    // TODO: Separate channel & registers

    using BasicComponents;
    using Shared;

    // http://wiki.nesdev.com/w/index.php/APU#Specification Pulse ($4000-4007)
    // http://wiki.nesdev.com/w/index.php/APU_Pulse
    internal sealed class PulseChannel : IMemory<Octet> { // Registers byte x 4 / 32bit

        private Octet[] quadlet = new Octet[4];
        public Octet this[int address] {
            get => quadlet[address & 3];
            set => quadlet[address & 3] = value;
        }
        public int Cells => quadlet.Length;

        // ------- Registers -------

        // quadlet[0] - $4000 | $4004 : DDLC VVVV : Duty (D), envelope loop / length counter halt (L), constant volume (C), volume/envelope (V)
        public byte Duty => (byte)(quadlet[0] >> 6 & 3);
        public bool LengthCounterHalt => quadlet[0][5];
        public bool ConstantVolume => quadlet[0][4];
        public byte EnvelopeDividerPeriodOrVolume => (byte)(quadlet[0] & 0b1111);

        // quadlet[1] - $4001 | $4005 : EPPP NSSS : Sweep unit: enabled (E), period (P), negate (N), shift (S)
        public bool SweepEnabled => quadlet[1][7];
        public byte SweepPeriod => (byte)((quadlet[1] >> 4) & 0b111);
        public bool SweepNegate => quadlet[1][3];
        public byte SweepShift => (byte)(quadlet[1] & 0b111);

        // quadlet[2] - $4002 | $4006 : TTTT TTTT : Timer low (T)
        // quadlet[3] - $4003 | $4007 : LLLL LTTT : Length counter load (L), timer high (T)

        public ushort Timer {
            get => (ushort)(((quadlet[3] & 0b111) << 8) | quadlet[2]);
            set {
                quadlet[2] = (byte)value;
                quadlet[3] = (byte)((quadlet[3] | 0b1111_1000) | ((value >> 8) & 0b111));
            }
        }
        public byte LengthCounterLoad => (byte)(quadlet[3] >> 3);

        // ------- Additional -------

        public bool Enabled { get; set; } = true;
        public bool EnvelopeLoop => LengthCounterHalt;

        // flag is shared with LengthCounterHalt
        public byte EnvelopeVolume { get; set; }
        public int EnvelopeCounter { get; set; }
        public int SweepPeriodCounter { get; set; }
        public int CurrentLengthCounter { get; set; }

        // http://wiki.nesdev.com/w/index.php/APU_Pulse 
        private double[] DutyMap { get; set; } = new double[] { .125, .25, .5, .75 };

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


        // http://wiki.nesdev.com/w/index.php/APU_Sweep
        public void TickSweep() { // ??

            if (SweepPeriodCounter == 0) {
                if (SweepEnabled) {
                    var periodAdjustment = Timer >> SweepShift;
                    int newPeriod;
                    if (SweepNegate) {
                        newPeriod = Timer - periodAdjustment;
                    }
                    else {
                        newPeriod = (Timer + periodAdjustment);
                    }
                    if (0 < newPeriod && newPeriod < 0x7FF && SweepShift != 0) {
                        Timer = (ushort)newPeriod;
                    }
                }
                SweepPeriodCounter = SweepPeriod;
            }
            else {
                SweepPeriodCounter--;
            }
        }

        // APU Envelope http://wiki.nesdev.com/w/index.php/APU_Envelope
        // Attack Decay Sustain Release (ADSR) Envelope https://en.wikipedia.org/wiki/Synthesizer#ADSR_envelope
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
                EnvelopeCounter = EnvelopeDividerPeriodOrVolume;
            }
            else {
                EnvelopeCounter--;
            }
        }

        public float GetPulseAudio(int timeInSamples, int sampleRate) {

            if (!Enabled || CurrentLengthCounter == 0) {
                return 0.0f;
            }

            var frequency = 102400.0 / Timer;
            var normalizedSampleTime = timeInSamples * frequency / sampleRate;

            var fractionalNormalizedSampleTime = normalizedSampleTime - (int)normalizedSampleTime;
            float dutyPulse = fractionalNormalizedSampleTime < DutyMap[Duty] ? 1 : -1;

            var volume = EnvelopeDividerPeriodOrVolume;
            if (!ConstantVolume) {
                volume = EnvelopeVolume;
            }
            return dutyPulse * volume / 15;
        }
    }
}