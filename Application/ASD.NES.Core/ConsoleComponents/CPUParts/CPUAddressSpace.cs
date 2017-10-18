using System;
using System.Linq;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    using BasicComponents;
    using ConsoleComponents.APUParts;
    using ConsoleComponents.PPUParts;
    using Helpers;
    using Shared;

    internal sealed class CPUAddressSpace : IMemory<Octet> {

        #region Singleton
        public static CPUAddressSpace Instance => instance.Value;

        private static readonly Lazy<CPUAddressSpace> instance = new Lazy<CPUAddressSpace>(() => new CPUAddressSpace());
        private CPUAddressSpace() { }
        #endregion

        private static readonly RefOctet[] internalMemory; // 0x0000 - 0x1FFF: cpuRam - 8 kb (2 kb mirror x4)
        private static readonly RegistersPPU registersPPU; // 0x2000 - 0x3FFF: ppuReg - 8 kb (8 b mirror x1024)
        private static readonly RegistersAPU registersAPU; // 0x4000 - 0x401F: apuReg - 32 b
        private static IMemory<Octet> externalMemory;      // 0x4020 - 0xFFFF: cartrg - 49120 b


        public Octet this[int address] {
            get => Read(address);
            set => Write(address, value);
        }
        public int Cells => 1024 * 64;

        public RegistersPPU RegistersPPU => registersPPU;
        public bool Nmi {
            get => externalMemory[0xFFFA][0];
            set => externalMemory[0xFFFA] = (Octet)(value ? externalMemory[0xFFFA] | 1 : externalMemory[0xFFFA] & ~1);
        }

        static CPUAddressSpace() {
            internalMemory = new Octet[2048].Wrap().Repeat(4).ToArray();
            registersPPU = new RegistersPPU();
            registersAPU = new RegistersAPU();
        }

        public void SetExternalMemory(IMemory<Octet> boardMemory) {
            externalMemory = boardMemory;
        }

        public Octet Read(int address) {
            if (address < 0x2000) {
                return internalMemory[address];
            }
            if (address < 0x4000) {
                return registersPPU[address];
            }
            if (address < 0x4020) {
                return registersAPU[address];
            }
            return externalMemory[address];
        }

        public void Write(int address, Octet value) {
            if (address < 0x2000) {
                internalMemory[address].Value = value;
            }
            else if (address < 0x4000 || address == 0x4014) {
                registersPPU[address] = value;
            }
            else if (address < 0x4020) {
                registersAPU[address] = value;
            }
            else {
                externalMemory[address] = value;
            }
        }
    }
}