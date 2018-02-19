using System;

namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Channels {

    using BasicComponents;
    using Registers;

    // TODO: Refactor childs
    internal abstract class AudioChannel {

        public AudioChannel(AudioChannelRegisters registers, int clockSpeed, int sampleRate) {

            r = registers; r.Changed += OnRegisterChanged;

            Exception Overflow(string param)
                => new OverflowException($"{param} can't be less than or equal to 0");

            ClockSpeed = clockSpeed > 0 ? clockSpeed : throw Overflow(nameof(clockSpeed));
            SampleRate = sampleRate > 0 ? sampleRate : throw Overflow(nameof(sampleRate));
        }

        // -- fields

        protected AudioChannelRegisters r;


        // -- properties

        public IMemory<byte> Registers => r;

        protected double ClockSpeed { get; }
        protected double SampleRate { get; }

        protected double SampleCount { get; set; }
        protected double Frequency { get; set; }
        protected double RenderedWaveLength { get; set; }

        public virtual int LengthCounter { get; protected set; }
        protected virtual bool LengthCounterDisabled { get; }
        protected virtual bool EnvelopeDecayDisabled { get; }
        protected virtual byte Volume { get; }
        protected virtual int Timer { get; set; }
        protected virtual byte LengthIndex { get; }

        protected int EnvelopeCounter { get; set; }
        protected byte EnvelopeVolume { get; set; }


        // -- methods

        public abstract void OnRegisterChanged(int address);

        protected virtual void UpdateFrequency() {
            Frequency = ClockSpeed / ((Timer + 1) * 0x10);
            RenderedWaveLength = SampleRate / Frequency;
        }

        // The length counter provides automatic duration control for the NES APU waveform channels.
        // http://wiki.nesdev.com/w/index.php/APU_Length_Counter
        public virtual void TickLength() {
            if (!LengthCounterDisabled) {
                LengthCounter--;
                if (LengthCounter < 0) {
                    LengthCounter = 0;
                }
            }
        }

        public virtual void TickSweep() { }

        // APU Envelope http://wiki.nesdev.com/w/index.php/APU_Envelope
        // ADSR Envelope https://en.wikipedia.org/wiki/Synthesizer#ADSR_envelope
        public virtual void TickEnvelope() {
            if (EnvelopeCounter == 0) {
                if (EnvelopeVolume == 0) {
                    if (LengthCounterDisabled) {
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

        public abstract float GetAudio();

        // -- static data

        // https://wiki.nesdev.com/w/index.php/APU_Length_Counter
        protected static byte[] waveLengths = {
            0x0A, 0xFE, 0x14, 0x02, 0x28, 0x04, 0x50, 0x06, 0xA0, 0x08, 0x3C, 0x0A, 0x0E, 0x0C, 0x1A, 0x0E,
            0x0C, 0x10, 0x18, 0x12, 0x30, 0x14, 0x60, 0x16, 0xC0, 0x18, 0x48, 0x1A, 0x10, 0x1C, 0x20, 0x1E
        };
    }
}