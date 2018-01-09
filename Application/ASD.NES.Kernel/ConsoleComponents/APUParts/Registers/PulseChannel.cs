using System;

namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Registers {

    // TODO: Separate channel & registers

    using BasicComponents;
    using Helpers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification Pulse ($4000-4007)
    // http://wiki.nesdev.com/w/index.php/APU_Pulse
    internal sealed class PulseChannel : IMemory<byte> { // Registers byte x 4 / 32bit

        private byte[] registers = new byte[4];
        public byte this[int address] {
            get => registers[address & 3];
            set => registers[address & 3] = value;
        }
        public int Cells => registers.Length;

        // ------- Registers -------

        // register[0] - $4000 | $4004 : DDLC VVVV : Duty (D), envelope loop / length counter halt (L), constant volume (C), volume/envelope (V)
        public byte Duty => (byte)(registers[0] >> 6 & 3);
        public bool LengthCounterHalt => registers[0].HasBit(5);
        public bool ConstantVolume => registers[0].HasBit(4);
        public byte EnvelopeDividerPeriodOrVolume => (byte)(registers[0] & 0b1111);

        // register[1] - $4001 | $4005 : EPPP NSSS : Sweep unit: enabled (E), period (P), negate (N), shift (S)
        public bool SweepEnabled => registers[1].HasBit(7);
        public byte SweepPeriod => (byte)((registers[1] >> 4) & 0b111);
        public bool SweepNegate => registers[1].HasBit(3);
        public byte SweepShift => (byte)(registers[1] & 0b111);

        // register[2] - $4002 | $4006 : TTTT TTTT : Timer low (T)
        // register[3] - $4003 | $4007 : LLLL LTTT : Length counter load (L), timer high (T)

        public ushort Timer {
            get => (ushort)(((registers[3] & 0b111) << 8) | registers[2]);
            set {
                registers[2] = (byte)value;
                registers[3] = (byte)((registers[3] | 0b1111_1000) | ((value >> 8) & 0b111));
            }
        }
        public byte LengthCounterLoad => (byte)(registers[3] >> 3);

        // ------- Additional -------

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
                        newPeriod = Timer + periodAdjustment;
                    }
                    if (newPeriod > 0 && newPeriod < 0x7FF && SweepShift != 0) {
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

            // http://wiki.nesdev.com/w/index.php/APU_Pulse
            // f = CPU / (16 * (t + 1)) | t = (CPU / (16 * f)) - 1 | 111860 ~ 1.789773 MHz / 16 / timer
            var frequency = 111860.0 / Timer;
            var normalizedSampleTime = timeInSamples * frequency / sampleRate;

            var fractionalNormalizedSampleTime = normalizedSampleTime - Math.Floor(normalizedSampleTime); // 0 ... 0.999
            float dutyPulse = fractionalNormalizedSampleTime < DutyMap[Duty] ? 1 : -1;

            var volume = EnvelopeDividerPeriodOrVolume;
            if (!ConstantVolume) {
                volume = EnvelopeVolume;
            }
            return dutyPulse * volume / 15;
        }
    }
}