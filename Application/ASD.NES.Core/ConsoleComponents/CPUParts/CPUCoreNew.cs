using System;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    using Shared;

    /// <summary> Emulation NMOS 6502 component of the CPU RP2A03 (Ricoh Processor 2A03) </summary>
    internal sealed class CPUCoreNew {

        private readonly CPUAddressSpace memory = CPUAddressSpace.Instance;
        private readonly RegistersCPU r;

        private readonly AddressingModeNew[] addressing;
        private readonly Action[] instruction;

        private AddressingModeNew mode, ACC_, IMM_, ZPG_, ZPX_, ZPY_, ABS_, ABX_, ABY_, IND_, IDX_, IDY_, REL_, IMP_, ____ = null;

        internal event Action Clock;

        private byte opcode;

        ///// <summary>
        ///// Memory operand. <para/>
        ///// ATTENTION! Reading or Writing - increases the "<see cref="RegistersCPU.PC"/>" and Invoke the "<see cref="Clock"/>" several times.
        ///// </summary>
        //private int M { get => mode.M; set => mode.M = value; }

        public CPUCoreNew(RegistersCPU registers) {

            r = registers;

            IMM_ = new IMM_(r, this);
            ZPG_ = new ZPG_(r, this); ZPX_ = new ZPX_(r, this); ZPY_ = new ZPY_(r, this);
            ABS_ = new ABS_(r, this); // ABX_ = new ABX_(r, this); ABY_ = new ABY_(r, this);
            // IND_ = new IND_(r, this); IDX_ = new IDX_(r, this); IDY_ = new IDY_(r, this);

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
                null, null, null, null, null, ORA,  ASL,  null, null, ORA,  null, null, null, ORA,  ASL,  null,
                null, null, null, null, null, ORA,  ASL,  null, null, null, null, null, null, null, null, null,
                JSR,  null, null, null, BIT,  AND,  ROL,  null, null, AND,  null, null, BIT,  AND,  ROL,  null,
                null, null, null, null, null, AND,  ROL,  null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, EOR,  LSR,  null, null, EOR,  null, null, JMP,  EOR,  LSR,  null,
                null, null, null, null, null, EOR,  LSR,  null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, ADC,  ROR,  null, null, ADC,  null, null, null, ADC,  ROR,  null,
                null, null, null, null, null, ADC,  ROR,  null, null, null, null, null, null, null, null, null,
                null, null, null, null, STY,  STA,  STX,  null, null, null, null, null, STY,  STA,  STX,  null,
                null, null, null, null, STY,  STA,  STX,  null, null, null, null, null, null, null, null, null,
                LDY,  null, LDX,  null, LDY,  LDA,  LDX,  null, null, LDA,  null, null, LDY,  LDA,  LDX,  null,
                null, null, null, null, LDY,  LDA,  LDX,  null, null, null, null, null, null, null, null, null,
                CPY,  null, null, null, CPY,  CMP,  DEC,  null, null, CMP,  null, null, CPY,  CMP,  DEC,  null,
                null, null, null, null, null, CMP,  DEC,  null, null, null, null, null, null, null, null, null,
                CPX,  null, null, null, CPX,  SBC,  INC,  null, null, SBC,  null, null, CPX,  SBC,  INC,  null,
                null, null, null, null, null, SBC,  INC,  null, null, null, null, null, null, null, null, null,
            };

            // min PC inc = 1;
            // min Cycles inc = 2;
        }

        public bool HaveInstruction(byte opcode) => instruction[opcode] != null;

        #region Step
        public void Step() {
            ReadOpcode();
            SwitchMode();
            ExecuteInstruction();
        }
        private void ReadOpcode() {
            ClockTime(); opcode = memory[r.PC];
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
        #endregion

        #region Instructions
        #region Arithmetic

        /// <summary> Add Memory to Accumulator with Carry </summary>
        private void ADC() { // IMM, ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            var result = r.A + m + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);
        }

        /// <summary> Subtract Memory from Accumulator with Carry </summary>
        private void SBC() { // IMM, ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            var result = r.A + (m ^ 0xFF) + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);
        }
        #endregion
        #region Arithmetic: Increment / Decrement

        /// <summary> Increment Memory </summary>
        private void INC() { // ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            m++;

            r.PS.UpdateSigned(m);
            r.PS.UpdateZero(m);

            mode.Write(m);
        }

        /// <summary> Decrement Memory </summary>
        private void DEC() { // ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            m--;

            r.PS.UpdateSigned(m);
            r.PS.UpdateZero(m);

            mode.Write(m);
        }
        #endregion
        #region Arithmetic: Bit Operations

        /// <summary> AND Memory with Accumulator </summary>
        private void AND() { // IMM, ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            r.A &= m;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);
        }

        /// <summary> OR Memory with Accumulator </summary>
        private void ORA() { // IMM, ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            r.A |= m;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);
        }

        /// <summary> XOR Memory with Accumulator </summary>
        private void EOR() { // IMM, ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            r.A ^= m;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);
        }

        /// <summary> Arithmetic Shift Left one bit (Memory or Accumulator) </summary>
        private void ASL() { // ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            r.PS.C.Set((m >> 7) & 1);

            m <<= 1;

            r.PS.UpdateSigned(m);
            r.PS.UpdateZero(m);

            mode.Write(m);
        }

        /// <summary> Logical Shift Right one bit (Memory or Accumulator) </summary>
        private void LSR() { // ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            r.PS.C.Set((m >> 0) & 1);

            m >>= 1;

            r.PS.UpdateSigned(m);
            r.PS.UpdateZero(m);

            mode.Write(m);
        }

        /// <summary> Rotate Left one bit (Memory or Accumulator) </summary>
        private void ROL() { // ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            var carry = (m >> 7) & 1;

            m = (byte)((m << 1) | (r.PS.C >> 0));

            r.PS.C.Set(carry);
            r.PS.UpdateSigned(m);
            r.PS.UpdateZero(m);

            mode.Write(m);
        }

        /// <summary> Rotate Right one bit (Memory or Accumulator) </summary>
        private void ROR() { // ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            var carry = (m >> 0) & 1;

            m = (byte)((m >> 1) | (r.PS.C << 7));

            r.PS.C.Set(carry);
            r.PS.UpdateSigned(m);
            r.PS.UpdateZero(m);

            mode.Write(m);
        }

        /// <summary> Test bits in Memory with Accumulator </summary>
        private void BIT() { // ZPG, ABS
            var m = (byte)mode.Read();

            r.PS.V.Set(r.A[6]);
            r.PS.UpdateSigned(m);
            r.PS.UpdateZero(r.A & m);
        }
        #endregion
        #region Arithmetic: Compare

        /// <summary> Compare Memory and Accumulator </summary>
        private void CMP() { // IMM, ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            var result = r.A - m;

            r.PS.C.Set(result >= 0);
            r.PS.UpdateSigned(result);
            r.PS.UpdateZero(result);
        }

        /// <summary> Compare Memory and Index Register X </summary>
        private void CPX() { // IMM, ZPG, ABS
            var m = (byte)mode.Read();

            var result = r.X - m;

            r.PS.C.Set(result >= 0);
            r.PS.UpdateSigned(result);
            r.PS.UpdateZero(result);
        }

        /// <summary> Compare Memory and Index Register Y </summary>
        private void CPY() { // IMM, ZPG, ABS
            var m = (byte)mode.Read();

            var result = r.Y - m;

            r.PS.C.Set(result >= 0);
            r.PS.UpdateSigned(result);
            r.PS.UpdateZero(result);
        }
        #endregion
        #region Jumps

        /// <summary> Jump to new location </summary>
        private void JMP() { // ABS // Not impl.: JMP.IND has a bug where the indirect address wraps the page boundary
            r.PC = mode.ReadAddress();
            r.PC--;      // <--- !!!!!!!!!! // TODO: fix
        }

        /// <summary> Jump to Subroutine (and saving return address) </summary>
        private void JSR() { // ABS
            Push16((ushort)(r.PC + 2));
            r.PC = mode.ReadAddress();
            r.PC--;      // <--- !!!!!!!!!! // TODO: fix
            ClockTime(); // <--- !!!!!!!!!! // TODO: fix
        }
        #endregion
        #region Load / Store

        /// <summary> Load Accumulator from Memory </summary>
        private void LDA() { // IMM, ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            r.A = m;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);
        }

        /// <summary> Load Index Register X from Memory </summary>
        private void LDX() { // IMM, ZPG, ZPY, ABS
            var m = (byte)mode.Read();

            r.X = m;

            r.PS.UpdateSigned(r.X);
            r.PS.UpdateZero(r.X);
        }

        /// <summary> Load Index Register Y from Memory </summary>
        private void LDY() { // IMM, ZPG, ZPX, ABS
            var m = (byte)mode.Read();

            r.Y = m;

            r.PS.UpdateSigned(r.Y);
            r.PS.UpdateZero(r.Y);
        }

        /// <summary> Store Accumulator to Memory </summary>
        private void STA() { // ZPG, ZPX, ABS
            mode.WriteOnly(r.A);
        }

        /// <summary> Store Index Register X to Memory </summary>
        private void STX() { // ZPG, ZPY, ABS
            mode.WriteOnly(r.X);
        }

        /// <summary> Store Index Register Y to Memory </summary>
        private void STY() { // ZPG, ZPX, ABS
            mode.WriteOnly(r.Y);
        }
        #endregion
        #region Stack

        /// <summary> Isn't instruction </summary>
        private void Push16(Hextet value) {
            Push(value.H); Push(value.L);
        }

        /// <summary> Isn't instruction </summary>
        private ushort Pull16() {
            return (ushort)(Pull() | Pull() << 8);
        }

        /// <summary> Isn't instruction </summary>
        private void Push(Octet value) {
            ClockTime();                  // <--- !!!!!!!!!! // TODO: move Clock's
            memory[0x100 + r.SP] = value;
            r.SP -= 1;
        }

        /// <summary> Isn't instruction </summary>
        private Octet Pull() {
            ClockTime();                  // <--- !!!!!!!!!! // TODO: move Clock's
            r.SP += 1;
            return memory[0x100 + r.SP];
        }
        #endregion
        #endregion
    }
}