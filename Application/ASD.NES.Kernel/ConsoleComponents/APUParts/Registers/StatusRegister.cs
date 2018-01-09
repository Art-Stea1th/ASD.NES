namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Registers {

    using Helpers;

    // https://wiki.nesdev.com/w/index.php/APU
    internal sealed class StatusRegister {

        private byte octet;
        public byte Value { get => octet; set => octet = value; }

        public bool PulseAEnabled { get => octet.HasBit(0); set => octet = octet.WithChangedBit(0, value); }
        public bool PulseBEnabled { get => octet.HasBit(1); set => octet = octet.WithChangedBit(1, value); }
        public bool TriangleEnabled { get => octet.HasBit(2); set => octet = octet.WithChangedBit(2, value); }
        public bool NoiseEnabled { get => octet.HasBit(3); set => octet = octet.WithChangedBit(3, value); }
        public bool DmcEnabled { get => octet.HasBit(4); set => octet = octet.WithChangedBit(4, value); }

        // bit 5 - unused

        public bool FrameInterrupt => octet.HasBit(6); // readonly
        public bool DmcInterrupt => octet.HasBit(7);   // readonly
    }
}