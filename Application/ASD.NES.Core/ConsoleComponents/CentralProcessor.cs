using System;

namespace ASD.NES.Core.ConsoleComponents {

    using CPUParts;
    using Shared;

    internal sealed class CentralProcessor {

        private static readonly CPUAddressSpace memory = CPUAddressSpace.Instance;

        private CPUCore core;
        private CPUCoreNew coreNew;
        private RegistersCPU registers;

        public CPUAddressSpace AddressSpace => memory;

        public event Action Clock {
            add => coreNew.Clock += value;
            remove => coreNew.Clock -= value;
        }

        public CentralProcessor() {
            Initialize();
        }

        private void Initialize() {

            registers = new RegistersCPU();

            core = new CPUCore(registers);
            coreNew = new CPUCoreNew(registers);
        }

        public int Step() {

            if (memory.Nmi) {
                memory.Nmi = false;
                JumpToNMIVector();
                coreNew.ClockTime();
                return 0;
            }

            var opcode = memory[registers.PC];

            if (coreNew.HaveInstruction(opcode)) {
                coreNew.Step();
                return 0;
            }
            return core.Execute(opcode);
        }

        public void ColdBoot() {

            registers.PS.B.Set(true);
            registers.PS.I.Set(true);
            registers.PS.U.Set(true);
            registers.A = registers.X = registers.Y = 0;
            registers.SP = 0xFD;

            JumpToResetVector();

            memory[0x4017] = 0x00;
            memory[0x4015] = 0x00;
        }

        public void WarmBoot() {

            JumpToResetVector();

            registers.SP -= 3;
            registers.PS.I.Set(true);
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
        public ushort ReadX2(Hextet address) {
            return Hextet.Make(memory[address + 1], memory[address]);
        }
        private void PushStack(Octet val) {
            memory[0x100 + registers.SP] = val;
            registers.SP -= 1;
        }
        private void PushStack16(Hextet val) {
            PushStack(val.H);
            PushStack(val.L);
        }
        #endregion
    }
}