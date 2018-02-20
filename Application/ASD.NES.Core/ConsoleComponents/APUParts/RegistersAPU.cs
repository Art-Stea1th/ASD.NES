namespace ASD.NES.Core.ConsoleComponents.APUParts {

    using BasicComponents;
    using Registers;

    internal sealed class RegistersAPU : IMemory<byte> {

        public AudioChannelRegisters PulseA { get; }
        public AudioChannelRegisters PulseB { get; }
        public AudioChannelRegisters Triangle { get; }
        public AudioChannelRegisters Noise { get; }
        public AudioChannelRegisters Modulation { get; }

        public AudioChannelStatusRegister Status { get; }

        public byte this[int address] { get => 0; set => Write(address, value); } // - write only
        public int Cells => 20; // 0x4000 - 0x4013 // +1 - 'status register' at 0x4015   

        public RegistersAPU() {

            PulseA = new AudioChannelRegisters();
            PulseB = new AudioChannelRegisters();
            Triangle = new AudioChannelRegisters();
            Noise = new AudioChannelRegisters();
            Modulation = new AudioChannelRegisters();

            Status = new AudioChannelStatusRegister();
        }

        private void Write(int address, byte value) {

            if (address >= 0x4000 && address <= 0x4003) {
                PulseA[address] = value;
            }
            else if (address >= 0x4004 && address <= 0x4007) {
                PulseB[address] = value;
            }
            else if (address >= 0x4008 && address <= 0x400B) {
                Triangle[address] = value;
            }
            else if (address >= 0x400C && address <= 0x400F) {
                Noise[address] = value;
            }
            else if (address >= 0x4010 && address <= 0x4013) {
                Modulation[address] = value;
            }
            else if (address == 0x4015) {
                Status.Value = value;
            }
            else {
                return;
            }
        }
    }
}