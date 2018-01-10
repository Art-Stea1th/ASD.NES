﻿namespace ASD.NES.Kernel.ConsoleComponents {

    using CPUParts;
    using Helpers;

    internal sealed class CentralProcessor {

        private static readonly CPUAddressSpace memory = CPUAddressSpace.Instance;

        private CPUCore core;
        private RegistersCPU registers;

        public CPUAddressSpace AddressSpace => memory;

        public CentralProcessor() {
            Initialize();
        }

        private void Initialize() {
            registers = new RegistersCPU();
            core = new CPUCore(registers);
        }

        public int Step() {

            if (memory.Nmi) {
                memory.Nmi = false;
                JumpToNMIVector();
                return 1;
            }

            var opcode = memory[registers.PC];
            return core.Execute(opcode);
        }

        public void ColdBoot() {

            registers.PS.B = true;
            registers.PS.I = true;
            registers.PS.U = true;
            registers.A = registers.X = registers.Y = 0;
            registers.SP = 0xFD;

            JumpToResetVector();

            memory[0x4017] = 0x00;
            memory[0x4015] = 0x00;
        }

        public void WarmBoot() {

            JumpToResetVector();

            registers.SP -= 3;
            registers.PS.I = true;
            memory[0x4015] = 0x00;
        }

        private void JumpToResetVector() {
            registers.PC = ReadX2(0xFFFC);
        }

        private void JumpToNMIVector() {

            PushStack16(registers.PC);
            PushStack(registers.PS);

            registers.PC = ReadX2(0xFFFA);
        }

        #region Helpers
        public ushort ReadX2(ushort address) {
            return BitOperations.MakeInt16(memory[address + 1], memory[address]);
        }
        private void PushStack(byte val) {
            memory[0x100 + registers.SP] = val;
            registers.SP -= 1;
        }
        private void PushStack16(ushort val) {
            PushStack(val.H());
            PushStack(val.L());
        }
        #endregion
    }
}