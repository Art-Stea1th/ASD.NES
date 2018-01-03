using System;

namespace ASD.NES.Kernel.ConsoleComponents.CPUParts {

    using APUParts;
    using BasicComponents;
    using CartridgeComponents.Boards;
    using PPUParts;
    using Shared;

    internal sealed class CPUAddressSpace : IMemory<Octet> {

        #region Singleton
        public static CPUAddressSpace Instance => instance.Value;

        private static readonly Lazy<CPUAddressSpace> instance = new Lazy<CPUAddressSpace>(() => new CPUAddressSpace());
        private CPUAddressSpace() { }
        #endregion

        private static readonly Octet[] internalMemory;    // 0x0000 - 0x1FFF: cpuRam - 8 kb (2 kb mirror x4) - [ZeroPage, Stack, WRAM]
        private static readonly RegistersPPU registersPPU; // 0x2000 - 0x3FFF: ppuReg - 8 kb (8 b mirror x1024) + 1b (0x4014)
        private static readonly RegistersAPU registersAPU; // 0x4000 - 0x4013: apuReg - 20 b + 1 b (0x4015)
        private static readonly InputPort registersInput;  // 0x4016 - 0x4017: Input - 2 b
                                                           // 0x4018 - 0x4019: ??
        private static Board externalMemory;               // 0x4020 - 0xFFFF: cartrg - 49120 b

        public Octet this[int address] {
            get => Read(address);
            set => Write(address, value);
        }
        public int Cells => 1024 * 64;

        public bool Nmi {
            get => this[0xFFFA][0];
            set => this[0xFFFA] = (Octet)(value ? this[0xFFFA] | 1 : this[0xFFFA] & ~1);
        }

        public RegistersPPU RegistersPPU => registersPPU;
        public RegistersAPU RegistersAPU => registersAPU;
        public InputPort InputPort => registersInput;

        static CPUAddressSpace() {
            internalMemory = new Octet[2048];
            registersPPU = new RegistersPPU();
            registersAPU = new RegistersAPU();
            registersInput = new InputPort();
        }

        public void SetExternalMemory(Board boardMemory) {
            externalMemory = boardMemory;
        }

        private Octet Read(int address) {
            if (address < 0x2000) {
                return internalMemory[address & 0x7FF];
            }
            if (address < 0x4000 || address == 0x4014) {
                return registersPPU[address];
            }
            if (address < 0x4014 || address == 0x4015) {
                return registersAPU[address];
            }
            if (address == 0x4016 || address == 0x4017) {
                return registersInput[address];
            }
            return externalMemory[address];
        }

        private void Write(int address, Octet value) {
            if (address < 0x2000) {
                internalMemory[address & 0x7FF] = value;
            }
            else if (address < 0x4000 || address == 0x4014) {
                registersPPU[address] = value;
            }
            else if (address < 0x4014 || address == 0x4015) {
                registersAPU[address] = value;
            }
            else if (address == 0x4016 || address == 0x4017) {
                registersInput[address] = value;
            }
            else {
                externalMemory[address] = value;
            }
        }
    }
}