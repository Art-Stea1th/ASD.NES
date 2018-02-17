namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Registers {

    using Helpers;

    // https://wiki.nesdev.com/w/index.php/APU
    internal sealed class AudioChannelStatusRegister {

        private byte r;
        public byte Value { get => r; set => r = value; }

        public bool PulseAEnabled { get => r.HasBit(0); set => r = r.WithChangedBit(0, value); }
        public bool PulseBEnabled { get => r.HasBit(1); set => r = r.WithChangedBit(1, value); }
        public bool TriangleEnabled { get => r.HasBit(2); set => r = r.WithChangedBit(2, value); }
        public bool NoiseEnabled { get => r.HasBit(3); set => r = r.WithChangedBit(3, value); }
        public bool DmcEnabled { get => r.HasBit(4); set => r = r.WithChangedBit(4, value); }

        // bit 5 - unused

        public bool FrameInterrupt => r.HasBit(6); // readonly
        public bool DmcInterrupt => r.HasBit(7);   // readonly
    }
}