namespace ASD.NES.Kernel.ConsoleComponents.APUParts {

    using BasicComponents;
    using Registers;

    internal sealed class RegistersAPU : IMemory<byte> {

        // TODO: Separate channels & registers, Add 'base-classes' for them

        public PulseChannel PulseA { get; }
        public PulseChannel PulseB { get; }
        public TriangleChannel Triangle { get; }
        public NoiseChannel Noise { get; }
        public DeltaModulationChannel DeltaModulation { get; }

        public StatusRegister Status { get; }

        public byte this[int address] { get => 0; set => Write(address, value); } // - write only
        public int Cells => 20; // 0x4000 - 0x4013 // +1 - 'status register' at 0x4015 [out of range]

        // https://wiki.nesdev.com/w/index.php/APU_Length_Counter
        private byte[] lengthCounterLookupTable = new byte[] {
            0x0A, 0xFE, 0x14, 0x02, 0x28, 0x04, 0x50, 0x06, 0xA0, 0x08, 0x3C, 0x0A, 0x0E, 0x0C, 0x1A, 0x0E,
            0x0C, 0x10, 0x18, 0x12, 0x30, 0x14, 0x60, 0x16, 0xC0, 0x18, 0x48, 0x1A, 0x10, 0x1C, 0x20, 0x1E
        };

        public RegistersAPU() {
            PulseA = new PulseChannel();
            PulseB = new PulseChannel();
            Triangle = new TriangleChannel();
            Noise = new NoiseChannel();
            DeltaModulation = new DeltaModulationChannel();

            Status = new StatusRegister();
        }

        // TODO: Refactor
        private void Write(int address, byte value) {

            if (address >= 0x4000 && address <= 0x4007) {
                var pulse = address < 0x4004 ? PulseA : PulseB;
                pulse[address] = value;

                switch (address & 0b11) {
                    case 0b00:
                        pulse.EnvelopeVolume = 15;
                        pulse.EnvelopeCounter = pulse.EnvelopeDividerPeriodOrVolume;
                        break;
                    case 0b01:
                        pulse.SweepPeriodCounter = pulse.SweepPeriod;
                        break;
                    case 0b11:
                        pulse.CurrentLengthCounter = lengthCounterLookupTable[pulse.LengthCounterLoad];
                        break;
                }
            }
            else if (address >= 0x4008 && address <= 0x400B) {
                Triangle[address] = value;
                switch (address & 0b11) {
                    case 0b00:
                        Triangle.CurrentLinearCounter = Triangle.LinearCounterLoad;
                        break;
                    case 0b11:
                        Triangle.CurrentLengthCounter = lengthCounterLookupTable[Triangle.LengthCounterLoad];
                        Triangle.CurrentLinearCounter = Triangle.LinearCounterLoad;
                        break;
                }
            }
            else if (address >= 0x400C && address <= 0x400F) {
                Noise[address] = value;
                switch (address & 0b11) {
                    case 0b00:
                        Noise.EnvelopeVolume = 15;
                        Noise.EnvelopeCounter = Noise.EnvelopeDividerPeriodOrVolume;
                        break;
                    case 0b11:
                        Noise.CurrentLengthCounter = lengthCounterLookupTable[Noise.LengthCounterLoad];
                        break;
                }
            }
            else if (address >= 0x4010 && address <= 0x4013) {
                DeltaModulation[address] = value;
                switch (address & 0b11) {
                    case 0b00:
                        break;
                    case 0b01:
                        break;
                    case 0b10:
                        break;
                    case 0b11:
                        break;
                }
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