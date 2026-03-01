namespace ASD.NES.Core.ConsoleComponents {

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
            var opcode = memory[registers.PC];
            var ticks = core.Execute(opcode);

            if (memory.Nmi) {
                memory.Nmi = false;
                JumpToNMIVector();
                return 1;
            }
            if (memory.Irq && !registers.PS.I) {
                JumpToIRQVector();
                return 1;
            }
            return ticks;
        }

        public void ClearRAM() => CPUAddressSpace.ClearRAM();

        public void ColdBoot() {

            registers.A = registers.X = registers.Y = 0;

            registers.PS.B = true;
            registers.PS.I = true;
            registers.PS.U = true;

            registers.SP = 0xFD;

            JumpToResetVector();

            memory[0x4017] = 0x00;
            memory[0x4015] = 0x00;
            memory.Irq = false;
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

        private void JumpToIRQVector() {

            PushStack16(registers.PC);
            PushStack((byte)(((byte)registers.PS & 0xEF) | 0x20)); // B=0, U=1
            registers.PS.I = true;
            registers.PC = ReadX2(0xFFFE);
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