namespace ASD.NES.Kernel.ConsoleComponents.APUParts {

    using BasicComponents;
    using Registers;
    using Shared;

    internal sealed class RegistersAPU : IMemory<Octet> {

        public PulseChannel PulseA { get; }
        public PulseChannel PulseB { get; }
        public TriangleChannel Triangle { get; }

        public StatusRegister Status { get; }

        public Octet this[int address] { get => 0; set => Write(address, value); }
        public int Cells => 20;

        // https://wiki.nesdev.com/w/index.php/APU_Length_Counter
        private byte[] lengthCounterLookupTable = new byte[] {
            0x0A, 0xFE, 0x14, 0x02, 0x28, 0x04, 0x50, 0x06, 0xA0, 0x08, 0x3C, 0x0A, 0x0E, 0x0C, 0x1A, 0x0E,
            0x0C, 0x10, 0x18, 0x12, 0x30, 0x14, 0x60, 0x16, 0xC0, 0x18, 0x48, 0x1A, 0x10, 0x1C, 0x20, 0x1E
        };

        public RegistersAPU() {
            PulseA = new PulseChannel();
            PulseB = new PulseChannel();
            Triangle = new TriangleChannel();

            Status = new StatusRegister();
        }

        private void Write(int address, Octet value) {

            if (address >= 0x4000 && address <= 0x4007) {
                var pulse = address < 0x4004 ? PulseA : PulseB;
                pulse[address] = value;

                switch (address & 3) {
                    case 0:
                        pulse.EnvelopeVolume = 15;
                        pulse.EnvelopeCounter = pulse.EnvelopeDividerPeriodOrVolume;
                        break;
                    case 1:
                        pulse.SweepPeriodCounter = pulse.SweepPeriod;
                        break;
                    case 3:
                        pulse.CurrentLengthCounter = lengthCounterLookupTable[pulse.LengthCounterLoad];
                        break;
                    default:
                        break;
                }
            }
            else if (address >= 0x4008 && address <= 0x400B) {
                Triangle[address] = value;
                switch (address & 3) {
                    case 0:
                        Triangle.CurrentLinearCounter = Triangle.LinearCounterLoad;
                        break;
                    case 3:
                        Triangle.CurrentLengthCounter = lengthCounterLookupTable[Triangle.LengthCounterLoad];
                        Triangle.CurrentLinearCounter = Triangle.LinearCounterLoad;
                        break;
                    default:
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