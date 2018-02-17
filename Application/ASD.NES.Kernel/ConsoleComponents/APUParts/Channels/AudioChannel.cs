namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Channels {

    using BasicComponents;

    internal abstract class AudioChannel {

        public abstract IMemory<byte> Registers { get; }
        public abstract float GetAudio();

        public virtual void TickEnvelope() { }
        public virtual void TickLengthCounter() { }
        public virtual void TickSweep() { }

        public abstract void OnRegisterChanged(int address); // TMP

        // https://wiki.nesdev.com/w/index.php/APU_Length_Counter
        protected static byte[] lengthCounterLookupTable = {
            0x0A, 0xFE, 0x14, 0x02, 0x28, 0x04, 0x50, 0x06, 0xA0, 0x08, 0x3C, 0x0A, 0x0E, 0x0C, 0x1A, 0x0E,
            0x0C, 0x10, 0x18, 0x12, 0x30, 0x14, 0x60, 0x16, 0xC0, 0x18, 0x48, 0x1A, 0x10, 0x1C, 0x20, 0x1E
        };
    }
}