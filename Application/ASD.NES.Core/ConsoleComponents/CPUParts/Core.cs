using System;
using OldCode;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    using Helpers;

    /// <summary> Emulation NMOS 6502 component of the CPU RP2A03 (Ricoh Processor 2A03) </summary>
    internal sealed class Core {

        private const int _ = 0;

        private readonly OldMemoryBus bus = OldMemoryBus.Instance;
        private readonly Registers r;

        private readonly Func<int>[] instruction;
        private readonly AddressingMode[] addressing;
        private readonly ushort[] bytes;
        private readonly int[] cycles;

        private AddressingMode mode, ACC, IMM, ZPG, ZPX, ZPY, ABS, ABX, ABY, IND, IDX, IDY, REL, IMP, ___ = null;

        private ushort Address { get => mode.Address; set => mode.Address = value; }
        private byte M { get => mode.M; set => mode.M = value; }
        private bool PageCrossed => mode.PageCrossed;

        public Core(Registers registers) {

            r = registers;

            IMP = new IMP(r); IMM = new IMM(r); 
            ZPG = new ZPG(r); ZPX = new ZPX(r); ZPY = new ZPY(r);
            ABS = new ABS(r); ABX = new ABX(r); ABY = new ABY(r);
            IND = new IND(r); IDX = new IDX(r); IDY = new IDY(r);
            REL = new REL(r); ACC = new ACC(r); 

            /// must be serialized
            addressing = new AddressingMode[] {
                IMP, ___, ___, ___, ___, ___, ZPG, ___, ___, ___, ACC, ___, ___, ___, ABS, ___,
                REL, ___, ___, ___, ___, ___, ZPX, ___, IMP, ___, ___, ___, ___, ___, ABX, ___,
                ABS, IDX, ___, ___, ZPG, ZPG, ___, ___, ___, IMM, ___, ___, ABS, ABS, ___, ___,
                REL, IDY, ___, ___, ___, ZPX, ___, ___, ___, ABY, ___, ___, ___, ABX, ___, ___,
                ___, IDX, ___, ___, ___, ZPG, ___, ___, ___, IMM, ___, ___, ABS, ABS, ___, ___,
                REL, IDY, ___, ___, ___, ZPX, ___, ___, IMP, ABY, ___, ___, ___, ABX, ___, ___,
                ___, IDX, ___, ___, ___, ZPG, ___, ___, ___, IMM, ___, ___, IND, ABS, ___, ___,
                REL, IDY, ___, ___, ___, ZPX, ___, ___, ___, ABY, ___, ___, ___, ABX, ___, ___,
                ___, ___, ___, ___, ___, ___, ___, ___, IMP, ___, ___, ___, ___, ___, ___, ___,
                REL, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___,
                ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___,
                REL, ___, ___, ___, ___, ___, ___, ___, IMP, ___, ___, ___, ___, ___, ___, ___,
                IMM, IDX, ___, ___, ZPG, ZPG, ZPG, ___, IMP, IMM, IMP, ___, ABS, ABS, ABS, ___,
                REL, IDY, ___, ___, ___, ZPX, ZPX, ___, IMP, ABY, ___, ___, ___, ABX, ABX, ___,
                IMM, ___, ___, ___, ZPG, ___, ZPG, ___, IMP, ___, ___, ___, ABS, ___, ABS, ___,
                REL, ___, ___, ___, ___, ___, ZPX, ___, ___, ___, ___, ___, ___, ___, ABX, ___,
            };

            /// must be serialized
            instruction = new Func<int>[] {
                BRK,  null, null, null, null, null, ASL,  null, null, null, ASL,  null, null, null, ASL,  null,
                BPL,  null, null, null, null, null, ASL,  null, CLC,  null, null, null, null, null, ASL,  null,
                JSR,  AND,  null, null, BIT,  AND,  null, null, null, AND,  null, null, BIT,  AND,  null, null,
                BMI,  AND,  null, null, null, AND,  null, null, null, AND,  null, null, null, AND,  null, null,
                null, EOR,  null, null, null, EOR,  null, null, null, EOR,  null, null, JMP,  EOR,  null, null,
                BVC,  EOR,  null, null, null, EOR,  null, null, CLI,  EOR,  null, null, null, EOR,  null, null,
                null, ADC,  null, null, null, ADC,  null, null, null, ADC,  null, null, JMP,  ADC,  null, null,
                BVS,  ADC,  null, null, null, ADC,  null, null, null, ADC,  null, null, null, ADC,  null, null,
                null, null, null, null, null, null, null, null, DEY,  null, null, null, null, null, null, null,
                BCC,  null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                BCS,  null, null, null, null, null, null, null, CLV,  null, null, null, null, null, null, null,
                CPY,  CMP,  null, null, CPY,  CMP,  DEC,  null, INY,  CMP,  DEX,  null, CPY,  CMP,  DEC,  null,
                BNE,  CMP,  null, null, null, CMP,  DEC,  null, CLD,  CMP,  null, null, null, CMP,  DEC,  null,
                CPX,  null, null, null, CPX,  null, INC,  null, INX,  null, null, null, CPX,  null, INC,  null,
                BEQ,  null, null, null, null, null, INC,  null, null, null, null, null, null, null, INC,  null,
            };

            /// must be serialized
            bytes = new ushort[] {
                1, _, _, _, _, _, 2, _, _, _, 1, _, _, _, 3, _,
                0, _, _, _, _, _, 2, _, 1, _, _, _, _, _, 3, _,
                0, 2, _, _, 2, 2, _, _, _, 2, _, _, 3, 3, _, _,
                0, 2, _, _, _, 2, _, _, _, 3, _, _, _, 3, _, _,
                _, 2, _, _, _, 2, _, _, _, 2, _, _, 0, 3, _, _,
                0, 2, _, _, _, 2, _, _, 1, 3, _, _, _, 3, _, _,
                _, 2, _, _, _, 2, _, _, _, 2, _, _, 0, 3, _, _,
                0, 2, _, _, _, 2, _, _, _, 3, _, _, _, 3, _, _,
                _, _, _, _, _, _, _, _, 1, _, _, _, _, _, _, _,
                0, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                0, _, _, _, _, _, _, _, 1, _, _, _, _, _, _, _,
                2, 2, _, _, 2, 2, 2, _, 1, 2, 1, _, 3, 3, 3, _,
                0, 2, _, _, _, 2, 2, _, 1, 3, _, _, _, 3, 3, _,
                2, _, _, _, 2, _, 2, _, 1, _, _, _, 3, _, 3, _,
                0, _, _, _, _, _, 2, _, _, _, _, _, _, _, 3, _,
            };

            /// must be serialized
            cycles = new int[] {
                7, _, _, _, _, _, 5, _, _, _, 2, _, _, _, 6, _,
                2, _, _, _, _, _, 6, _, 2, _, _, _, _, _, 7, _,
                6, 6, _, _, 3, 3, _, _, _, 2, _, _, 4, 4, _, _,
                2, 5, _, _, _, 4, _, _, _, 4, _, _, _, 4, _, _,
                _, 6, _, _, _, 3, _, _, _, 2, _, _, 3, 4, _, _,
                2, 5, _, _, _, 4, _, _, 2, 4, _, _, _, 4, _, _,
                _, 6, _, _, _, 3, _, _, _, 2, _, _, 5, 4, _, _,
                2, 5, _, _, _, 4, _, _, _, 4, _, _, _, 4, _, _,
                _, _, _, _, _, _, _, _, 2, _, _, _, _, _, _, _,
                2, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                2, _, _, _, _, _, _, _, 2, _, _, _, _, _, _, _,
                2, 6, _, _, 3, 3, 5, _, 2, 2, 2, _, 4, 4, 6, _,
                2, 5, _, _, _, 4, 6, _, 2, 4, _, _, _, 4, 7, _,
                2, _, _, _, 3, _, 5, _, 2, _, _, _, 4, _, 6, _,
                2, _, _, _, _, _, 6, _, _, _, _, _, _, _, 7, _,
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

        /// <summary> Logical AND (Memory with Accumulator) </summary>
        private int AND() {

            var result = r.A & M;

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            return PageCrossed ? 1 : 0;
        }

        /// <summary> Arithmetic Shift Left one bit (Memory or Accumulator) </summary>
        private int ASL() {

            var result = M << 1;

            r.PS.UpdateCarry(result);

            M = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

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
        #region Arithmetic: Decrement

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
        #region Arithmetic: Increment

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
        #endregion
        #region Bit Operations

        /// <summary> Test bits in Memory with Accumulator </summary>
        private int BIT() {

            r.PS.V.Set(r.A.HasBit(6));
            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(r.A & M);

            return 0;
        }

        /// <summary> Exclusive Or Memory with Accumulator </summary>
        private int EOR() {

            r.A ^= M;

            r.PS.UpdateSigned(M);
            r.PS.UpdateZero(M);

            return PageCrossed ? 1 : 0;
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
        #endregion
        #region Jumps

        /// <summary> Force Break </summary>
        private int BRK() {

            PushStack16(r.PC);
            // r.PS.B.Set(true); // ?
            PushStack(r.PS);
            // r.PS.I.Set(true); // ?

            r.PC = BitOperations.MakeInt16(bus.Read(0xFFFF), bus.Read(0xFFFE));
            return 0;
        }

        /// <summary> Jump to new location </summary>
        private int JMP() { // Not impl.: JMP.IND has a bug where the indirect address wraps the page boundary
            r.PC = Address;            
            return 0;
        }

        /// <summary> Jump to Subroutine (and saving return address) </summary>
        private int JSR() {
            PushStack16((ushort)(r.PC + 2));
            r.PC = Address;
            return 0;
        }
        #endregion
        #region Stack        

        /// <summary> Isn't instruction </summary>
        private void PushStack16(ushort value) {
            PushStack(value.HOctet());
            PushStack(value.LOctet());
        }

        /// <summary> Isn't instruction </summary>
        private void PushStack(byte value) {
            bus.Write((ushort)(0x100 + r.SP), value);
            r.SP -= 1;
        }
        #endregion
        #endregion
    }
}