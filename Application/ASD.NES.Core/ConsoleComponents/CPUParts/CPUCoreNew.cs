using System;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    /// <summary> Emulation NMOS 6502 component of the CPU RP2A03 (Ricoh Processor 2A03) </summary>
    internal sealed class CPUCoreNew {

        private readonly CPUAddressSpace memory = CPUAddressSpace.Instance;
        private readonly RegistersCPU r;

        private readonly AddressingModeNew[] addressing;
        private readonly Action[] instruction;

        private AddressingModeNew mode, ACC_, IMM_, ZPG_, ZPX_, ZPY_, ABS_, ABX_, ABY_, IND_, IDX_, IDY_, REL_, IMP_, ____ = null;

        internal event Action Clock;

        private byte opcode;

        /// <summary>
        /// Memory operand. <para/>
        /// ATTENTION! Reading or Writing - increases the "<see cref="RegistersCPU.PC"/>" and Invoke the "<see cref="Clock"/>" several times.
        /// </summary>
        private int M { get => mode.M; set => mode.M = value; }

        public CPUCoreNew(RegistersCPU registers) {

            r = registers;

            IMM_ = new IMM_(r, this);
            ZPG_ = new ZPG_(r, this); ZPX_ = new ZPX_(r, this); ZPY_ = new ZPY_(r, this);
            ABS_ = new ABS_(r, this); ABX_ = new ABX_(r, this); ABY_ = new ABY_(r, this);
            IND_ = new IND_(r, this); IDX_ = new IDX_(r, this); IDY_ = new IDY_(r, this);

            addressing = new AddressingModeNew[] {
                IMP_, IDX_, ____, ____, ____, ZPG_, ZPG_, ____, IMP_, IMM_, ACC_, ____, ____, ABS_, ABS_, ____,
                REL_, IDY_, ____, ____, ____, ZPX_, ZPX_, ____, IMP_, ABY_, ____, ____, ____, ABX_, ABX_, ____,
                ABS_, IDX_, ____, ____, ZPG_, ZPG_, ZPG_, ____, IMP_, IMM_, ACC_, ____, ABS_, ABS_, ABS_, ____,
                REL_, IDY_, ____, ____, ____, ZPX_, ZPX_, ____, IMP_, ABY_, ____, ____, ____, ABX_, ABX_, ____,
                IMP_, IDX_, ____, ____, ____, ZPG_, ZPG_, ____, IMP_, IMM_, ACC_, ____, ABS_, ABS_, ABS_, ____,
                REL_, IDY_, ____, ____, ____, ZPX_, ZPX_, ____, IMP_, ABY_, ____, ____, ____, ABX_, ABX_, ____,
                IMP_, IDX_, ____, ____, ____, ZPG_, ZPG_, ____, IMP_, IMM_, ACC_, ____, IND_, ABS_, ABS_, ____,
                REL_, IDY_, ____, ____, ____, ZPX_, ZPX_, ____, IMP_, ABY_, ____, ____, ____, ABX_, ABX_, ____,
                ____, IDX_, ____, ____, ZPG_, ZPG_, ZPG_, ____, IMP_, ____, IMP_, ____, ABS_, ABS_, ABS_, ____,
                REL_, IDY_, ____, ____, ZPX_, ZPX_, ZPY_, ____, IMP_, ABY_, IMP_, ____, ____, ABX_, ____, ____,
                IMM_, IDX_, IMM_, ____, ZPG_, ZPG_, ZPG_, ____, IMP_, IMM_, IMP_, ____, ABS_, ABS_, ABS_, ____,
                REL_, IDY_, ____, ____, ZPX_, ZPX_, ZPY_, ____, IMP_, ABY_, IMP_, ____, ABX_, ABX_, ABY_, ____,
                IMM_, IDX_, ____, ____, ZPG_, ZPG_, ZPG_, ____, IMP_, IMM_, IMP_, ____, ABS_, ABS_, ABS_, ____,
                REL_, IDY_, ____, ____, ____, ZPX_, ZPX_, ____, IMP_, ABY_, ____, ____, ____, ABX_, ABX_, ____,
                IMM_, IDX_, ____, ____, ZPG_, ZPG_, ZPG_, ____, IMP_, IMM_, IMP_, ____, ABS_, ABS_, ABS_, ____,
                REL_, IDY_, ____, ____, ____, ZPX_, ZPX_, ____, IMP_, ABY_, ____, ____, ____, ABX_, ABX_, ____,
            };

            instruction = new Action[] {
                null, null, null, null, null, null, null, null, null, ORA,  null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, AND,  null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, EOR,  null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, ADC,  null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                LDY,  null, LDX,  null, null, null, null, null, null, LDA,  null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                CPY,  null, null, null, null, null, null, null, null, CMP,  null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                CPX,  null, null, null, null, null, null, null, null, SBC,  null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            };

            // min PC inc = 1;
            // min Cycles inc = 2;
        }

        public bool HaveInstruction(byte opcode) => instruction[opcode] != null;

        public void Step() {
            ReadOpcode();
            SwitchMode();
            ExecuteInstruction();
        }

        private void ReadOpcode() {
            opcode = memory[r.PC]; ClockTime();
        }

        private void SwitchMode() {
            mode = addressing[opcode];
        }

        private void ExecuteInstruction() {
            instruction[opcode]();
            r.PC++;                                   // b++;
        }

        internal void ClockTimeIf(bool condition) {
            if (condition) { ClockTime(); }
        }

        internal void ClockTime() => Clock?.Invoke(); // c++;

        #region Instructions
        #region Arithmetic

        /// <summary> Add Memory to Accumulator with Carry </summary>
        private void ADC() { // IMM

            var result = r.A + M + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);
        }

        /// <summary> Subtract Memory from Accumulator with Carry </summary>
        private void SBC() { // IMM

            var result = r.A + (M ^ 0xFF) + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);
        }
        #endregion
        #region Arithmetic: Bit Operations

        /// <summary> AND Memory with Accumulator </summary>
        private void AND() { // IMM

            r.A &= (byte)M;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);
        }

        /// <summary> OR Memory with Accumulator </summary>
        private void ORA() { // IMM

            r.A |= (byte)M;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);
        }

        /// <summary> XOR Memory with Accumulator </summary>
        private void EOR() { // IMM

            r.A ^= (byte)M;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);
        }
        #endregion
        #region Arithmetic: Compare

        /// <summary> Compare Memory and Accumulator </summary>
        private void CMP() { // IMM

            var result = r.A - M;

            r.PS.C.Set(result >= 0);
            r.PS.UpdateSigned(result);
            r.PS.UpdateZero(result);
        }

        /// <summary> Compare Memory and Index Register X </summary>
        private void CPX() { // IMM

            var result = r.X - M;

            r.PS.C.Set(result >= 0);
            r.PS.UpdateSigned(result);
            r.PS.UpdateZero(result);
        }

        /// <summary> Compare Memory and Index Register Y </summary>
        private void CPY() { // IMM

            var result = r.Y - M;

            r.PS.C.Set(result >= 0);
            r.PS.UpdateSigned(result);
            r.PS.UpdateZero(result);
        }
        #endregion
        #region Load / Store

        /// <summary> Load Accumulator from Memory </summary>
        private void LDA() { // IMM

            r.A = (byte)M;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);
        }

        /// <summary> Load Index Register X from Memory </summary>
        private void LDX() { // IMM

            r.X = (byte)M;

            r.PS.UpdateSigned(r.X);
            r.PS.UpdateZero(r.X);
        }

        /// <summary> Load Index Register Y from Memory </summary>
        private void LDY() { // IMM

            r.Y = (byte)M;

            r.PS.UpdateSigned(r.Y);
            r.PS.UpdateZero(r.Y);
        }
        #endregion
        #endregion
    }
}