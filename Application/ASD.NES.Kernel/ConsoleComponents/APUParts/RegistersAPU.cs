namespace ASD.NES.Kernel.ConsoleComponents.APUParts {

    using BasicComponents;
    using Helpers;
    using Shared;

    internal sealed class RegistersAPU : IMemory<Octet> { // Dummy

        private RefOctet[] registers = new Octet[20].Wrap();

        private Octet verticalClockSignalRegister = 0;

        public Octet this[int address] {
            get => Read(address);
            set => Write(address, value);
        }
        public int Cells => registers.Length;

        private Octet Read(int address) {
            if (address == 0x4015) {
                return verticalClockSignalRegister;
            }
            return registers[address & 0x1F];
        }

        private void Write(int address, Octet value) {
            if (address == 0x4015) {
                verticalClockSignalRegister = value;
            }
            else {
                registers[address & 0x1F].Value = value;
            }
        }
    }
}