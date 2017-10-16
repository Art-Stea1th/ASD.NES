using System;
using OldCode;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {
    using ASD.NES.Core.Shared;
    using Helpers;

    /// <summary> Emulation NMOS 6502 component of the CPU RP2A03 (Ricoh Processor 2A03) </summary>
    internal sealed class Core { // all core instructions must be rewritten into classes

        private const int _ = 0;

        private readonly OldMemoryBus bus = OldMemoryBus.Instance;
        private readonly RegistersCPU r;

        private readonly Func<int>[] instruction;
        private readonly AddressingMode[] addressing;
        private readonly ushort[] bytes;
        private readonly int[] cycles;

        private AddressingMode mode, ACC, IMM, ZPG, ZPX, ZPY, ABS, ABX, ABY, IND, IDX, IDY, REL, IMP, ___ = null;

        private Hextet Address { get => mode.Address; set => mode.Address = value; }
        private Octet M { get => mode.M; set => mode.M = value; }
        private bool PageCrossed => mode.PageCrossed;

        public Core(RegistersCPU registers) {

            r = registers;

            IMP = new IMP(r); IMM = new IMM(r);
            ZPG = new ZPG(r); ZPX = new ZPX(r); ZPY = new ZPY(r);
            ABS = new ABS(r); ABX = new ABX(r); ABY = new ABY(r);
            IND = new IND(r); IDX = new IDX(r); IDY = new IDY(r);
            REL = new REL(r); ACC = new ACC(r);

            /// must be serialized
            addressing = new AddressingMode[] {
                IMP, IDX, ___, ___, ___, ZPG, ZPG, ___, IMP, IMM, ACC, ___, ___, ABS, ABS, ___,
                REL, IDY, ___, ___, ___, ZPX, ZPX, ___, IMP, ABY, ___, ___, ___, ABX, ABX, ___,
                ABS, IDX, ___, ___, ZPG, ZPG, ZPG, ___, IMP, IMM, ACC, ___, ABS, ABS, ABS, ___,
                REL, IDY, ___, ___, ___, ZPX, ZPX, ___, IMP, ABY, ___, ___, ___, ABX, ABX, ___,
                IMP, IDX, ___, ___, ___, ZPG, ZPG, ___, IMP, IMM, ACC, ___, ABS, ABS, ABS, ___,
                REL, IDY, ___, ___, ___, ZPX, ZPX, ___, IMP, ABY, ___, ___, ___, ABX, ABX, ___,
                IMP, IDX, ___, ___, ___, ZPG, ZPG, ___, IMP, IMM, ACC, ___, IND, ABS, ABS, ___,
                REL, IDY, ___, ___, ___, ZPX, ZPX, ___, IMP, ABY, ___, ___, ___, ABX, ABX, ___,
                ___, IDX, ___, ___, ZPG, ZPG, ZPG, ___, IMP, ___, IMP, ___, ABS, ABS, ABS, ___,
                REL, IDY, ___, ___, ZPX, ZPX, ZPY, ___, IMP, ABY, IMP, ___, ___, ABX, ___, ___,
                IMM, IDX, IMM, ___, ZPG, ZPG, ZPG, ___, IMP, IMM, IMP, ___, ABS, ABS, ABS, ___,
                REL, IDY, ___, ___, ZPX, ZPX, ZPY, ___, IMP, ABY, IMP, ___, ABX, ABX, ABY, ___,
                IMM, IDX, ___, ___, ZPG, ZPG, ZPG, ___, IMP, IMM, IMP, ___, ABS, ABS, ABS, ___,
                REL, IDY, ___, ___, ___, ZPX, ZPX, ___, IMP, ABY, ___, ___, ___, ABX, ABX, ___,
                IMM, IDX, ___, ___, ZPG, ZPG, ZPG, ___, IMP, IMM, IMP, ___, ABS, ABS, ABS, ___,
                REL, IDY, ___, ___, ___, ZPX, ZPX, ___, IMP, ABY, ___, ___, ___, ABX, ABX, ___,
            };

            /// must be serialized
            instruction = new Func<int>[] {
                BRK,  ORA,  null, null, null, ORA,  ASL,  null, PHP,  ORA,  ASL,  null, null, ORA,  ASL,  null,
                BPL,  ORA,  null, null, null, ORA,  ASL,  null, CLC,  ORA,  null, null, null, ORA,  ASL,  null,
                JSR,  AND,  null, null, BIT,  AND,  ROL,  null, PLP,  AND,  ROL,  null, BIT,  AND,  ROL,  null,
                BMI,  AND,  null, null, null, AND,  ROL,  null, SEC,  AND,  null, null, null, AND,  ROL,  null,
                RTI,  EOR,  null, null, null, EOR,  LSR,  null, PHA,  EOR,  LSR,  null, JMP,  EOR,  LSR,  null,
                BVC,  EOR,  null, null, null, EOR,  LSR,  null, CLI,  EOR,  null, null, null, EOR,  LSR,  null,
                RTS,  ADC,  null, null, null, ADC,  ROR,  null, PLA,  ADC,  ROR,  null, JMP,  ADC,  ROR,  null,
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

            /// must be auto-count
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

            /// must be auto-count
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

        public bool HaveInstruction(byte opcode) => instruction[opcode] != null;

        public int Execute(byte opcode) {
            mode = addressing[opcode];
            var ticks = cycles[opcode];
            ticks += instruction[opcode]();
            r.PC += bytes[opcode];
            return ticks;
        }

        #region Instructions
        #region Arithmetic

        /// <summary> Add Memory to Accumulator with Carry </summary>
        private int ADC() {

            var result = r.A + M + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            return PageCrossed ? 1 : 0;
        }

        /// <summary> Subtract Memory from Accumulator with Carry </summary>
        private int SBC() {

            var result = r.A + (M ^ 0xFF) + r.PS.C;

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

            r.PS.C.Set(M[7]);

            M <<= 1;

            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(M);

            return 0;
        }

        /// <summary> Logical Shift Right one bit (Memory or Accumulator) </summary>
        private int LSR() {

            r.PS.C.Set(M[0]);

            M >>= 1;

            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(M);

            return 0;
        }

        /// <summary> Rotate Left one bit (Memory or Accumulator) </summary>
        private int ROL() {

            var carry = M[7];

            M = (byte)((M << 1) | r.PS.C);

            r.PS.C.Set(carry);
            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(M);

            return 0;
        }

        /// <summary> Rotate Right one bit (Memory or Accumulator) </summary>
        private int ROR() {

            var carry = M[0];

            M = (byte)((M >> 1) | (r.PS.C << 7));

            r.PS.C.Set(carry);
            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(M);

            return 0;
        }

        /// <summary> Test bits in Memory with Accumulator </summary>
        private int BIT() {

            r.PS.V.Set(r.A[6]);
            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(r.A & M);

            return 0;
        }
        #endregion
        #region Arithmetic: Compare

        /// <summary> Compare Memory and Accumulator </summary>
        private int CMP() {

            var result = r.A - M;

            r.PS.C.Set(result >= 0);
            r.PS.UpdateSigned(result);
            r.PS.UpdateZero(result);

            return PageCrossed ? 1 : 0;
        }

        /// <summary> Compare Memory and Index Register X </summary>
        private int CPX() {

            var result = r.X - M;

            r.PS.C.Set(result >= 0);
            r.PS.UpdateSigned(result);
            r.PS.UpdateZero(result);

            return 0;
        }

        /// <summary> Compare Memory and Index Register Y </summary>
        private int CPY() {

            var result = r.Y - M;

            r.PS.C.Set(result >= 0);
            r.PS.UpdateSigned(result);
            r.PS.UpdateZero(result);

            return 0;
        }
        #endregion
        #region Branch

        /// <summary> Branch if Carry Clear </summary>
        private int BCC() => BranchIf(r.PS.C == 0);

        /// <summary> Branch if Zero Clear </summary>
        private int BNE() => BranchIf(r.PS.Z == 0);

        /// <summary> Branch if Plus (if Signed Clear) </summary>
        private int BPL() => BranchIf(r.PS.S == 0);

        /// <summary> Branch if Overflow Clear </summary>
        private int BVC() => BranchIf(r.PS.V == 0);


        /// <summary> Branch if Carry Set </summary>
        private int BCS() => BranchIf(r.PS.C == 1);

        /// <summary> Branch if Zero Set </summary>
        private int BEQ() => BranchIf(r.PS.Z == 1);

        /// <summary> Branch if Minus (if Signed Set) </summary>
        private int BMI() => BranchIf(r.PS.S == 1);

        /// <summary> Branch if Overflow Set </summary>
        private int BVS() => BranchIf(r.PS.V == 1);


        /// <summary> Isn't instruction </summary>
        private int BranchIf(bool condition) {

            if (condition) {

                var addressNew = (ushort)(Address + 2 + (sbyte)bus.Read((ushort)(Address + 1)));
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
            r.PS.C.Set(false);
            return 0;
        }

        /// <summary> Clear Decimal flag </summary>
        private int CLD() {
            r.PS.D.Set(false);
            return 0;
        }

        /// <summary> Clear Interrupt Disable flag </summary>
        private int CLI() {
            r.PS.I.Set(false);
            return 0;
        }

        /// <summary> Clear Overflow flag </summary>
        private int CLV() {
            r.PS.V.Set(false);
            return 0;
        }


        /// <summary> Set Carry flag </summary>
        private int SEC() {
            r.PS.C.Set(true);
            return 0;
        }

        /// <summary> Set Decimal flag </summary>
        private int SED() {
            r.PS.D.Set(true);
            return 0;
        }

        /// <summary> Set Interrupt Disable flag </summary>
        private int SEI() {
            r.PS.I.Set(true);
            return 0;
        }
        #endregion
        #region Jumps

        /// <summary> Force Break </summary>
        private int BRK() {

            Push16(r.PC); // r.PS.B.Set(true); // ?
            Push(r.PS);   // r.PS.I.Set(true); // ?

            r.PC = Hextet.Make(bus.Read(0xFFFF), bus.Read(0xFFFE));
            return 0;
        }

        /// <summary> Jump to new location </summary>
        private int JMP() { // Not impl.: JMP.IND has a bug where the indirect address wraps the page boundary
            r.PC = Address;
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
        private int LDA() {

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
        private void Push16(Hextet value) {
            Push(value.H); Push(value.L);
        }

        /// <summary> Isn't instruction </summary>
        private ushort Pull16() {
            return (ushort)(Pull() | Pull() << 8);
        }

        /// <summary> Isn't instruction </summary>
        private void Push(Octet value) {
            bus.Write((ushort)(0x100 + r.SP), value);
            r.SP -= 1;
        }

        /// <summary> Isn't instruction </summary>
        private Octet Pull() {
            r.SP += 1;
            return bus.Read((ushort)(0x100 + r.SP));
        }
        #endregion
        #endregion
    }
}