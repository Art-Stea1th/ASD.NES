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

            ClockSpeed = clockSpeed > 0 ? (uint)clockSpeed : throw Overflow(nameof(clockSpeed));
            SampleRate = sampleRate > 0 ? (uint)sampleRate : throw Overflow(nameof(sampleRate));
        }

        // -- fields

        protected AudioChannelRegisters r;


        // -- properties

        public IMemory<byte> Registers => r;

        protected uint ClockSpeed { get; }
        protected uint SampleRate { get; }

        protected double SampleCount { get; set; }
        protected double Frequency { get; set; }
        protected double RenderedWaveLength { get; set; }

        protected virtual int Timer { get; set; }


        // -- methods

        public abstract void OnRegisterChanged(int address);

        protected virtual void UpdateFrequency() {
            Frequency = ClockSpeed / ((Timer + 1) * 0x10);
            RenderedWaveLength = SampleRate / Frequency;
        }

        public virtual void TickEnvelope() { }
        public virtual void TickLengthCounter() { }
        public virtual void TickSweep() { }

        public abstract float GetAudio();


        // -- static data

        // https://wiki.nesdev.com/w/index.php/APU_Length_Counter
        protected static byte[] waveLengths = {
            0x0A, 0xFE, 0x14, 0x02, 0x28, 0x04, 0x50, 0x06, 0xA0, 0x08, 0x3C, 0x0A, 0x0E, 0x0C, 0x1A, 0x0E,
            0x0C, 0x10, 0x18, 0x12, 0x30, 0x14, 0x60, 0x16, 0xC0, 0x18, 0x48, 0x1A, 0x10, 0x1C, 0x20, 0x1E
        };
    }
}