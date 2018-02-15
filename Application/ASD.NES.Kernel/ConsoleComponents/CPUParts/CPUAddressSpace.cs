using System;

namespace ASD.NES.Kernel.ConsoleComponents.CPUParts {

    using APUParts;
    using BasicComponents;
    using CartridgeComponents.Boards;
    using Helpers;
    using PPUParts;

    internal sealed class CPUAddressSpace : IMemory<byte> {

        #region Singleton
        public static CPUAddressSpace Instance => instance.Value;

        private static readonly Lazy<CPUAddressSpace> instance = new Lazy<CPUAddressSpace>(() => new CPUAddressSpace());
        private CPUAddressSpace() { }
        #endregion

        private static readonly byte[] internalMemory;     // 0x0000 - 0x1FFF: cpuRam - 8 kb (2 kb mirror x4) - [ZeroPage, Stack, WRAM]
        private static readonly RegistersPPU registersPPU; // 0x2000 - 0x3FFF: ppuReg - 8 kb (8 b mirror x1024) + 1b (0x4014)
        private static readonly RegistersAPU registersAPU; // 0x4000 - 0x4013: apuReg - 20 b (4 b x5: Pulse x2, Triangle, Noise, DMC) + 1 b [Status: 0x4015]
        private static readonly InputPort registersInput;  // 0x4016 - 0x4017: Input - 2 b
                                                           // 0x4018 - 0x4019: ??
        private static Board externalMemory;               // 0x4020 - 0xFFFF: cartrg - 49120 b

        public byte this[int address] {
            get => Read(address);
            set => Write(address, value);
        }
        public int Cells => 1024 * 64;

        public bool Nmi {
            get => Read(0xFFFA).HasBit(0);
            set { var nmi = Read(0xFFFA); Write(0xFFFA, nmi.WithChangedBit(0, value)); }
        }

        public RegistersPPU RegistersPPU => registersPPU;
        public RegistersAPU RegistersAPU => registersAPU;
        public InputPort InputPort => registersInput;

        static CPUAddressSpace() {
            internalMemory = new byte[2048];
            registersPPU = new RegistersPPU();
            registersAPU = new RegistersAPU();
            registersInput = new InputPort();
        }

        public void SetExternalMemory(Board boardMemory) {
            externalMemory = boardMemory;
        }

        private byte Read(int address) {
            if (address < 0x2000) {
                return internalMemory[address & 0x7FF];
            }
            if (address < 0x4000 || address == 0x4014) {
                return registersPPU[address];
            }
            if (address < 0x4014 || address == 0x4015) {
                return 0; // registersAPU[address]; // - write only
            }
            if (address == 0x4016 || address == 0x4017) {
                return registersInput[address];
            }
            return externalMemory[address];
        }

        private void Write(int address, byte value) {
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