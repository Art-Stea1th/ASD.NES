namespace ASD.NES.Core.ConsoleComponents.APUParts {

    using BasicComponents;
    using Helpers;
    using Shared;

    internal sealed class RegistersAPU : IMemory<Octet> { // Dummy

        private RefOctet[] registers = new Octet[32].Wrap();
        public Octet this[int address] {
            get => registers[address & 0x1F];
            set => registers[address & 0x1F].Value = value;
        }
        public int Cells => registers.Length;
    }
}