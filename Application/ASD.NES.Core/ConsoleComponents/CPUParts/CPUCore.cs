﻿using System;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    using Helpers;

    /// <summary> Emulation NMOS 6502 component of the CPU RP2A03 (Ricoh Processor 2A03) </summary>
    internal sealed class CPUCore { // all core instructions must be rewritten into a classes

        private const int _ = 0;

        private readonly CPUAddressSpace memory = CPUAddressSpace.Instance;
        private readonly RegistersCPU r;

        private readonly Func<int>[] instruction;
        private readonly AddressingMode[] addressing;
        private readonly ushort[] bytes;
        private readonly int[] cycles;

        private AddressingMode mode, acc_, imm_, zpg_, zpx_, zpy_, abs_, abx_, aby_, ind_, idx_, idy_, rel_, imp_, ____ = null;

        private ushort Address { get => mode.Address; set => mode.Address = value; }
        private byte M { get => mode.M; set => mode.M = value; }
        private bool PageCrossed => mode.PageCrossed;

        public CPUCore(RegistersCPU registers) {

            r = registers;

            imp_ = new IMP(r); imm_ = new IMM(r);
            zpg_ = new ZPG(r); zpx_ = new ZPX(r); zpy_ = new ZPY(r);
            abs_ = new ABS(r); abx_ = new ABX(r); aby_ = new ABY(r);
            ind_ = new IND(r); idx_ = new IDX(r); idy_ = new IDY(r);
            rel_ = new REL(r); acc_ = new ACC(r);

            addressing = new AddressingMode[] {
                imp_, idx_, ____, ____, ____, zpg_, zpg_, ____, imp_, imm_, acc_, ____, ____, abs_, abs_, ____,
                rel_, idy_, ____, ____, ____, zpx_, zpx_, ____, imp_, aby_, ____, ____, ____, abx_, abx_, ____,
                abs_, idx_, ____, ____, zpg_, zpg_, zpg_, ____, imp_, imm_, acc_, ____, abs_, abs_, abs_, ____,
                rel_, idy_, ____, ____, ____, zpx_, zpx_, ____, imp_, aby_, ____, ____, ____, abx_, abx_, ____,
                imp_, idx_, ____, ____, ____, zpg_, zpg_, ____, imp_, imm_, acc_, ____, abs_, abs_, abs_, ____,
                rel_, idy_, ____, ____, ____, zpx_, zpx_, ____, imp_, aby_, ____, ____, ____, abx_, abx_, ____,
                imp_, idx_, ____, ____, ____, zpg_, zpg_, ____, imp_, imm_, acc_, ____, ind_, abs_, abs_, ____,
                rel_, idy_, ____, ____, ____, zpx_, zpx_, ____, imp_, aby_, ____, ____, ____, abx_, abx_, ____,
                ____, idx_, ____, ____, zpg_, zpg_, zpg_, ____, imp_, ____, imp_, ____, abs_, abs_, abs_, ____,
                rel_, idy_, ____, ____, zpx_, zpx_, zpy_, ____, imp_, aby_, imp_, ____, ____, abx_, ____, ____,
                imm_, idx_, imm_, ____, zpg_, zpg_, zpg_, ____, imp_, imm_, imp_, ____, abs_, abs_, abs_, ____,
                rel_, idy_, ____, ____, zpx_, zpx_, zpy_, ____, imp_, aby_, imp_, ____, abx_, abx_, aby_, ____,
                imm_, idx_, ____, ____, zpg_, zpg_, zpg_, ____, imp_, imm_, imp_, ____, abs_, abs_, abs_, ____,
                rel_, idy_, ____, ____, ____, zpx_, zpx_, ____, imp_, aby_, ____, ____, ____, abx_, abx_, ____,
                imm_, idx_, ____, ____, zpg_, zpg_, zpg_, ____, imp_, imm_, imp_, ____, abs_, abs_, abs_, ____,
                rel_, idy_, ____, ____, ____, zpx_, zpx_, ____, imp_, aby_, ____, ____, ____, abx_, abx_, ____,
            };

            instruction = new Func<int>[] {
                BRK,  ORA,  null, null, null, ORA,  ASL,  null, PHP,  ORA,  ASL,  null, null, ORA,  ASL,  null,
                BPL,  ORA,  null, null, null, ORA,  ASL,  null, CLC,  ORA,  null, null, null, ORA,  ASL,  null,
                JSR,  AND,  null, null, BIT,  AND,  ROL,  null, PLP,  AND,  ROL,  null, BIT,  AND,  ROL,  null,
                BMI,  AND,  null, null, null, AND,  ROL,  null, SEC,  AND,  null, null, null, AND,  ROL,  null,
                RTI,  EOR,  null, null, null, EOR,  LSR,  null, PHA,  EOR,  LSR,  null, JMP,  EOR,  LSR,  null,
                BVC,  EOR,  null, null, null, EOR,  LSR,  null, CLI,  EOR,  null, null, null, EOR,  LSR,  null,
                RTS,  ADC,  null, null, null, ADC,  ROR,  null, PLA,  ADC,  ROR,  null, JMI,  ADC,  ROR,  null,
                BVS,  ADC,  null, null, null, ADC,  ROR,  null, SEI,  ADC,  null, null, null, ADC,  ROR,  null,
                null, STA,  null, null, STY,  STA,  STX,  null, DEY,  null, TXA,  null, STY,  STA,  STX,  null,
                BCC,  STA,  null, null, STY,  STA,  STX,  null, TYA,  STA,  TXS,  null, null, STA,  null, null,
                LDY,  LDA,  LDX,  null, LDY,  LDA,  LDX,  null, TAY,  LDA,  TAX,  null, LDY,  LDA,  LDX,  null,
                BCS,  LDA,  null, null, LDY,  LDA,  LDX,  null, CLV,  LDA,  TSX,  null, LDY,  LDA,  LDX,  null,
                CPY,  CMP,  null, null, CPY,  CMP,  DEC,  null, INY,  CMP,  DEX,  null, CPY,  CMP,  DEC,  null,
                BNE,  CMP,  null, null, null, CMP,  DEC,  null, CLD,  CMP,  null, null, null, CMP,  DEC,  null,
                CPX,  SBC,  null, null, CPX,  SBC,  INC,  null, INX,  SBC,  NOP,  null, CPX,  SBC,  INC,  null,
                BEQ,  SBC,  null, null, null, SBC,  INC,  null, SED,  SBC,  null, null, null, SBC,  INC,  null,
            };

            bytes = new ushort[] {
                1, 2, _, _, _, 2, 2, _, 1, 2, 1, _, _, 3, 3, _,
                0, 2, _, _, _, 2, 2, _, 1, 3, _, _, _, 3, 3, _,
                0, 2, _, _, 2, 2, 2, _, 1, 2, 1, _, 3, 3, 3, _,
                0, 2, _, _, _, 2, 2, _, 1, 3, _, _, _, 3, 3, _,
                0, 2, _, _, _, 2, 2, _, 1, 2, 1, _, 0, 3, 3, _,
                0, 2, _, _, _, 2, 2, _, 1, 3, _, _, _, 3, 3, _,
                1, 2, _, _, _, 2, 2, _, 1, 2, 1, _, 0, 3, 3, _,
                0, 2, _, _, _, 2, 2, _, 1, 3, _, _, _, 3, 3, _,
                _, 2, _, _, 2, 2, 2, _, 1, _, 1, _, 3, 3, 3, _,
                0, 2, _, _, 2, 2, 2, _, 1, 3, 1, _, _, 3, _, _,
                2, 2, 2, _, 2, 2, 2, _, 1, 2, 1, _, 3, 3, 3, _,
                0, 2, _, _, 2, 2, 2, _, 1, 3, 1, _, 3, 3, 3, _,
                2, 2, _, _, 2, 2, 2, _, 1, 2, 1, _, 3, 3, 3, _,
                0, 2, _, _, _, 2, 2, _, 1, 3, _, _, _, 3, 3, _,
                2, 2, _, _, 2, 2, 2, _, 1, 2, 1, _, 3, 3, 3, _,
                0, 2, _, _, _, 2, 2, _, 1, 3, _, _, _, 3, 3, _,
            };

            cycles = new int[] {
                7, 6, _, _, _, 3, 5, _, 3, 2, 2, _, _, 4, 6, _,
                2, 5, _, _, _, 4, 6, _, 2, 4, _, _, _, 4, 7, _,
                6, 6, _, _, 3, 3, 5, _, 4, 2, 2, _, 4, 4, 6, _,
                2, 5, _, _, _, 4, 6, _, 2, 4, _, _, _, 4, 7, _,
                6, 6, _, _, _, 3, 5, _, 3, 2, 2, _, 3, 4, 6, _,
                2, 5, _, _, _, 4, 6, _, 2, 4, _, _, _, 4, 7, _,
                6, 6, _, _, _, 3, 5, _, 4, 2, 2, _, 5, 4, 6, _,
                2, 5, _, _, _, 4, 6, _, 2, 4, _, _, _, 4, 7, _,
                _, 6, _, _, 3, 3, 3, _, 2, _, 2, _, 4, 4, 4, _,
                2, 6, _, _, 4, 4, 4, _, 2, 5, 2, _, _, 5, _, _,
                2, 6, 2, _, 3, 3, 3, _, 2, 2, 2, _, 4, 4, 4, _,
                2, 5, _, _, 4, 4, 4, _, 2, 4, 2, _, 4, 4, 4, _,
                2, 6, _, _, 3, 3, 5, _, 2, 2, 2, _, 4, 4, 6, _,
                2, 5, _, _, _, 4, 6, _, 2, 4, _, _, _, 4, 7, _,
                2, 6, _, _, 3, 3, 5, _, 2, 2, 2, _, 4, 4, 6, _,
                2, 5, _, _, _, 4, 6, _, 2, 4, _, _, _, 4, 7, _,
            };
        }

        public int Execute(byte opcode) {

            mode = addressing[opcode];

            var ticks = 0;

            if (instruction[opcode] != null) {
                ticks = cycles[opcode];
                ticks += instruction[opcode]();
                r.PC += bytes[opcode];
            }
            else {
                ticks = cycles[0xEA];
                ticks += NOP();
                r.PC += bytes[0xEA];
            }
            return ticks;
        }

        #region Instructions
        #region Arithmetic

        /// <summary> Add Memory to Accumulator with Carry </summary>
        private int ADC() {

            var result = r.A + M + r.PS.C.ToInt();

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            return PageCrossed ? 1 : 0;
        }

        /// <summary> Subtract Memory from Accumulator with Carry </summary>
        private int SBC() {

            var result = r.A + (M ^ 0xFF) + r.PS.C.ToInt();

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            return PageCrossed ? 1 : 0;
        }
        #endregion
        #region Arithmetic: Increment / Decrement

        /// <summary> Increment Memory </summary>
        private int INC() {

            M++;

            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(M);

            return 0;
        }

        /// <summary> Increment Index Register X </summary>
        private int INX() {

            r.X++;

            r.PS.UpdateSigned(r.X);
            r.PS.UpdateZero(r.X);

            return 0;
        }

        /// <summary> Increment Index Register Y </summary>
        private int INY() {

            r.Y++;

            r.PS.UpdateSigned(r.Y);
            r.PS.UpdateZero(r.Y);

            return 0;
        }

        /// <summary> Decrement Memory </summary>
        private int DEC() {

            M--;

            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(M);

            return 0;
        }

        /// <summary> Decrement Index Register X </summary>
        private int DEX() {

            r.X--;

            r.PS.UpdateSigned(r.X);
            r.PS.UpdateZero(r.X);

            return 0;
        }

        /// <summary> Decrement Index Register Y </summary>
        private int DEY() {

            r.Y--;

            r.PS.UpdateSigned(r.Y);
            r.PS.UpdateZero(r.Y);

            return 0;
        }
        #endregion
        #region Arithmetic: Bit Operations

        /// <summary> AND Memory with Accumulator </summary>
        private int AND() {

            r.A &= M;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            return PageCrossed ? 1 : 0;
        }

        /// <summary> OR Memory with Accumulator </summary>
        private int ORA() {

            r.A |= M;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            return PageCrossed ? 1 : 0;
        }

        /// <summary> XOR Memory with Accumulator </summary>
        private int EOR() {

            r.A ^= M;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            return PageCrossed ? 1 : 0;
        }

        /// <summary> Arithmetic Shift Left one bit (Memory or Accumulator) </summary>
        private int ASL() {

            r.PS.C = M.HasBit(7);

            M <<= 1;

            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(M);

            return 0;
        }

        /// <summary> Logical Shift Right one bit (Memory or Accumulator) </summary>
        private int LSR() {

            r.PS.C = M.HasBit(0);

            M >>= 1;

            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(M);

            return 0;
        }

        /// <summary> Rotate Left one bit (Memory or Accumulator) </summary>
        private int ROL() {

            var carry = M.HasBit(7);

            M = (byte)((M << 1) | r.PS.C.ToInt());

            r.PS.C = carry;
            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(M);

            return 0;
        }

        /// <summary> Rotate Right one bit (Memory or Accumulator) </summary>
        private int ROR() {

            var carry = M.HasBit(0);

            M = (byte)((M >> 1) | (r.PS.C.ToInt() << 7));

            r.PS.C = carry;
            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(M);

            return 0;
        }

        /// <summary> Test bits in Memory with Accumulator </summary>
        private int BIT() {

            r.PS.V = r.A.HasBit(6);
            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(r.A & M);

            return 0;
        }
        #endregion
        #region Arithmetic: Compare

        /// <summary> Compare Memory and Accumulator </summary>
        private int CMP() {

            var result = r.A - M;

            r.PS.C = result >= 0;
            r.PS.UpdateSigned(result);
            r.PS.UpdateZero(result);

            return PageCrossed ? 1 : 0;
        }

        /// <summary> Compare Memory and Index Register X </summary>
        private int CPX() {

            var result = r.X - M;

            r.PS.C = result >= 0;
            r.PS.UpdateSigned(result);
            r.PS.UpdateZero(result);

            return 0;
        }

        /// <summary> Compare Memory and Index Register Y </summary>
        private int CPY() {

            var result = r.Y - M;

            r.PS.C = result >= 0;
            r.PS.UpdateSigned(result);
            r.PS.UpdateZero(result);

            return 0;
        }
        #endregion
        #region Branch

        /// <summary> Branch if Carry Clear </summary>
        private int BCC() => BranchIf(!r.PS.C);

        /// <summary> Branch if Zero Clear </summary>
        private int BNE() => BranchIf(!r.PS.Z);

        /// <summary> Branch if Plus (if Signed Clear) </summary>
        private int BPL() => BranchIf(!r.PS.S);

        /// <summary> Branch if Overflow Clear </summary>
        private int BVC() => BranchIf(!r.PS.V);


        /// <summary> Branch if Carry Set </summary>
        private int BCS() => BranchIf(r.PS.C);

        /// <summary> Branch if Zero Set </summary>
        private int BEQ() => BranchIf(r.PS.Z);

        /// <summary> Branch if Minus (if Signed Set) </summary>
        private int BMI() => BranchIf(r.PS.S);

        /// <summary> Branch if Overflow Set </summary>
        private int BVS() => BranchIf(r.PS.V);


        /// <summary> Isn't instruction </summary>
        private int BranchIf(bool condition) {

            if (condition) {

                var addressNew = (ushort)(Address + 2 + (sbyte)((byte)memory[Address + 1]));
                var cycles = mode.SamePage(Address, addressNew) ? 1 : 2;

                Address = addressNew;
                return cycles;
            }
            Address += 2;
            return 0;
        }
        #endregion
        #region Flag Manipulations

        /// <summary> Clear Carry flag </summary>
        private int CLC() {
            r.PS.C = false;
            return 0;
        }

        /// <summary> Clear Decimal flag </summary>
        private int CLD() {
            r.PS.D = false;
            return 0;
        }

        /// <summary> Clear Interrupt Disable flag </summary>
        private int CLI() {
            r.PS.I = false;
            return 0;
        }

        /// <summary> Clear Overflow flag </summary>
        private int CLV() {
            r.PS.V = false;
            return 0;
        }


        /// <summary> Set Carry flag </summary>
        private int SEC() {
            r.PS.C = true;
            return 0;
        }

        /// <summary> Set Decimal flag </summary>
        private int SED() {
            r.PS.D = true;
            return 0;
        }

        /// <summary> Set Interrupt Disable flag </summary>
        private int SEI() {
            r.PS.I = true;

            memory.Nmi = false; // !
            r.PS.B = false;     // !?

            return 0;
        }
        #endregion
        #region Jumps

        /// <summary> Force Break </summary>
        private int BRK() {

            Push16(r.PC); //r.PS.B = true; // ?
            Push(r.PS);   //r.PS.I = true; // ?

            r.PC = BitOperations.MakeInt16(memory[0xFFFF], memory[0xFFFE]);
            return 0;
        }

        /// <summary> Jump to new location </summary>
        private int JMP() {
            r.PC = Address;
            return 0;
        }

        /// <summary> Jump to new location (spec. for Indirect 0x6C, JMP.IND has a bug) </summary>
        private int JMI() {

            var addr = memory[r.PC + 1];
            var low = addr;
            var hig = (addr & 0xFF) == 0xFF ? addr & 0xFF00 : addr + 1;

            r.PC = BitOperations.MakeInt16(memory[hig], memory[low]);
            return 0;
        }

        /// <summary> Jump to Subroutine (and saving return address) </summary>
        private int JSR() {
            Push16((ushort)(r.PC + 2));
            r.PC = Address;
            return 0;
        }

        /// <summary> Return from Interrupt </summary>
        private int RTI() {
            r.PS.SetNew(Pull());
            r.PC = Pull16();
            return 0;
        }

        /// <summary> Return from Subroutine </summary>
        private int RTS() {
            r.PC = Pull16();
            return 0;
        }

        /// <summary> No Operation </summary>
        private int NOP() {
            return 0;
        }
        #endregion
        #region Load / Store

        /// <summary> Load Accumulator from Memory </summary>
        private int LDA() { // bugged ??

            r.A = M;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            return PageCrossed ? 1 : 0;
        }

        /// <summary> Load Index Register X from Memory </summary>
        private int LDX() {

            r.X = M;

            r.PS.UpdateSigned(r.X);
            r.PS.UpdateZero(r.X);

            return PageCrossed ? 1 : 0;
        }

        /// <summary> Load Index Register Y from Memory </summary>
        private int LDY() {

            r.Y = M;

            r.PS.UpdateSigned(r.Y);
            r.PS.UpdateZero(r.Y);

            return PageCrossed ? 1 : 0;
        }


        /// <summary> Store Accumulator to Memory </summary>
        private int STA() {
            M = r.A;
            return 0;
        }

        /// <summary> Store Index Register X to Memory </summary>
        private int STX() {
            M = r.X;
            return 0;
        }

        /// <summary> Store Index Register Y to Memory </summary>
        private int STY() {
            M = r.Y;
            return 0;
        }
        #endregion
        #region Transfer

        /// <summary> Transfer Accumulator to Index Register X </summary>
        private int TAX() {

            r.X = r.A;

            r.PS.UpdateSigned(r.X);
            r.PS.UpdateZero(r.X);

            return 0;
        }

        /// <summary> Transfer Accumulator to Index Register Y </summary>
        private int TAY() {

            r.Y = r.A;

            r.PS.UpdateSigned(r.Y);
            r.PS.UpdateZero(r.Y);

            return 0;
        }

        /// <summary> Transfer Stack Pointer to Index Register X </summary>
        private int TSX() {

            r.X = r.SP;

            r.PS.UpdateSigned(r.X);
            r.PS.UpdateZero(r.X);

            return 0;
        }


        /// <summary> Transfer Index Register X to Accumulator</summary>
        private int TXA() {

            r.A = r.X;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            return 0;
        }

        /// <summary> Transfer Index Register Y to Accumulator</summary>
        private int TYA() {

            r.A = r.Y;

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            return 0;
        }

        /// <summary> Transfer Index Register X to Stack Pointer</summary>
        private int TXS() {

            r.SP = r.X;
            return 0;
        }
        #endregion
        #region Stack        

        /// <summary> Push Accumulator on Stack </summary>
        private int PHA() {
            Push(r.A);
            return 0;
        }

        /// <summary> Push Processor Status on Stack </summary>
        private int PHP() {
            Push(r.PS);
            return 0;
        }

        /// <summary> Pull Accumulator on Stack </summary>
        private int PLA() {
            r.A = Pull(); // r.PS.UpdateSigned(r.A); r.PS.UpdateZero(r.A); // ???
            return 0;
        }

        /// <summary> Pull Processor Status on Stack </summary>
        private int PLP() {
            r.PS.SetNew(Pull());
            return 0;
        }

        /// <summary> Isn't instruction </summary>
        private void Push16(ushort value) {
            Push(value.H()); Push(value.L());
        }

        /// <summary> Isn't instruction </summary>
        private ushort Pull16() {
            return (ushort)(Pull() | Pull() << 8);
        }

        /// <summary> Isn't instruction </summary>
        private void Push(byte value) {
            memory[0x100 + r.SP] = value;
            r.SP -= 1;
        }

        /// <summary> Isn't instruction </summary>
        private byte Pull() {
            r.SP += 1;
            return memory[0x100 + r.SP];
        }
        #endregion
        #endregion
    }
}