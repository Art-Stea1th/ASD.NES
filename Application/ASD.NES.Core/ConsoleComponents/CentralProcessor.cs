using OldCode;

namespace ASD.NES.Core.ConsoleComponents {

    using CPUParts;
    using Shared;

    internal sealed class CentralProcessor {

        private static readonly OldMemoryBus bus = OldMemoryBus.Instance;

        private RInt8[] zeroPage, stack, wram;
        private RInt8 res, nmi, irq, brk;

        private Core core;
        private Registers registers;

        public CentralProcessor() {
            Initialize();
        }

        private void Initialize() {

            zeroPage = bus.GetReferenceRange(0, 0x100);
            stack = bus.GetReferenceRange(0x100, 0x100);
            wram = bus.GetReferenceRange(0x200, 0x600);

            res = bus.GetReference(0xFFFC);
            nmi = bus.GetReference(0xFFFA);
            irq = brk = bus.GetReference(0xFFFE);

            registers = new Registers();

            core = new Core(registers);
        }

        public int Step() {

            if (nmi[0]) {
                nmi[0] = false;
                JumpToNMIVector();
                return 1;
            }

            var opcode = bus.Read(registers.PC);
            return core.Execute(opcode);
        }

        public void ColdBoot() {

            registers.PS.B.Set(true);
            registers.PS.I.Set(true);
            registers.PS.U.Set(true);
            registers.A = registers.X = registers.Y = 0;
            registers.SP = 0xFD;

            JumpToResetVector();

            bus.Write(0x4017, 0x00);
            bus.Write(0x4015, 0x00);
        }

        public void WarmBoot() {

            JumpToResetVector();

            registers.SP -= 3;
            registers.PS.I.Set(true);
            bus.Write(0x4015, 0x00);
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
            return (ushort)((bus.Read((ushort)(address + 1)) << 8) | bus.Read(address));
        }
        private void PushStack(byte val) {
            stack[registers.SP].Value = val;
            registers.SP -= 1;
        }
        private void PushStack16(ushort val) {
            stack[registers.SP].Value = (byte)(val >> 8);
            registers.SP -= 1;
            stack[registers.SP].Value = (byte)val;
            registers.SP -= 1;
        }
        #endregion
    }
}