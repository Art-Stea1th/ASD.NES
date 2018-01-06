namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Registers {

    using Shared;

    // https://wiki.nesdev.com/w/index.php/APU
    internal sealed class StatusRegister {

        private Octet octet;
        public Octet Value { get => octet; set => octet = value; }

        public bool PulseAEnabled { get => octet[0]; set => octet[0] = value; }
        public bool PulseBEnabled { get => octet[1]; set => octet[1] = value; }
        public bool TriangleEnabled { get => octet[2]; set => octet[2] = value; }
        public bool NoiseEnabled { get => octet[3]; set => octet[3] = value; }
        public bool DmcEnabled { get => octet[4]; set => octet[4] = value; }

        // bit 5 - unused

        public bool FrameInterrupt => octet[6]; // readonly
        public bool DmcInterrupt => octet[7];   // readonly
    }
}