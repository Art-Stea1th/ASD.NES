namespace ASD.NES.Kernel.ConsoleComponents.APUParts {

    using BasicComponents;
    using Registers;
    using Shared;

    internal enum FrameCounterMode { FourStep = 0, FiveStep = 1 }

    internal sealed class RegistersAPU : IMemory<Octet> {

        public PulseChannelRegister PulseA { get; private set; }
        public PulseChannelRegister PulseB { get; private set; }

        public Octet this[int address] { get => 0; set => Write(address, value); }
        public int Cells => 20;

        // https://wiki.nesdev.com/w/index.php/APU_Length_Counter
        private byte[] lengthCounterLookupTable = new byte[] {
            0x0A, 0xFE, 0x14, 0x02, 0x28, 0x04, 0x50, 0x06, 0xA0, 0x08, 0x3C, 0x0A, 0x0E, 0x0C, 0x1A, 0x0E,
            0x0C, 0x10, 0x18, 0x12, 0x30, 0x14, 0x60, 0x16, 0xC0, 0x18, 0x48, 0x1A, 0x10, 0x1C, 0x20, 0x1E
        };

        private FrameCounterMode frameCounterMode;
        private bool irqInhibit;

        public RegistersAPU() {
            PulseA = new PulseChannelRegister();
            PulseB = new PulseChannelRegister();
        }

        private void Write(int address, Octet value) {

            if (address >= 0x4000 && address <= 0x4007) {
                var pulse = address < 0x4004 ? PulseA : PulseB;
                pulse[address] = value;
            }
            else if (address == 0x4015) {
                return;
            }
            else {
                return;
            }
        }
    }
}