using System;
using OldCode;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    /// <summary> Emulation NMOS 6502 component of the CPU RP2A03 (Ricoh Processor 2A03) </summary>
    internal sealed class Core {

        private const int _ = 0;

        private readonly OldMemoryBus bus = OldMemoryBus.Instance;
        private readonly Registers r;

        private readonly Func<int>[] instruction;
        private readonly AddressingMode[] addressing;
        private readonly ushort[] bytes;
        private readonly int[] cycles;

        private AddressingMode mode, ACC, IMM, ZPG, ZPX, ZPY, ABS, ABX, ABY, INX, INY, ___ = null;

        private byte M { get => mode.M; set => mode.M = value; }
        private bool PageCrossed => mode.PageCrossed;

        public Core(Registers registers) {
            r = registers;

            instruction = new Func<int>[] {
                null, null, null, null, null, null, ASL,  null, null, null, ASL,  null, null, null, ASL,  null,
                null, null, null, null, null, null, ASL,  null, null, null, null, null, null, null, ASL,  null,
                null, AND,  null, null, null, AND,  null, null, null, AND,  null, null, null, AND,  null, null,
                null, AND,  null, null, null, AND,  null, null, null, AND,  null, null, null, AND,  null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, ADC,  null, null, null, ADC,  null, null, null, ADC,  null, null, null, ADC,  null, null,
                null, ADC,  null, null, null, ADC,  null, null, null, ADC,  null, null, null, ADC,  null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            };

            ACC = new ACC(r);
            IMM = new IMM(r);
            ZPG = new ZPG(r); ZPX = new ZPX(r); ZPY = new ZPY(r);
            ABS = new ABS(r); ABX = new ABX(r); ABY = new ABY(r);
            INX = new INX(r); INY = new INY(r);

            addressing = new AddressingMode[] {
                ___, ___, ___, ___, ___, ___, ZPG, ___, ___, ___, ACC, ___, ___, ___, ABS, ___,
                ___, ___, ___, ___, ___, ___, ZPX, ___, ___, ___, ___, ___, ___, ___, ABX, ___,
                ___, INX, ___, ___, ___, ZPG, ___, ___, ___, IMM, ___, ___, ___, ABS, ___, ___,
                ___, INY, ___, ___, ___, ZPX, ___, ___, ___, ABY, ___, ___, ___, ABX, ___, ___,
                ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___,
                ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___,
                ___, INX, ___, ___, ___, ZPG, ___, ___, ___, IMM, ___, ___, ___, ABS, ___, ___,
                ___, INY, ___, ___, ___, ZPX, ___, ___, ___, ABY, ___, ___, ___, ABX, ___, ___,
                ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___,
                ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___,
                ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___,
                ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___,
                ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___,
                ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___,
                ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___,
                ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___,
            };

            bytes = new ushort[] {
                _, _, _, _, _, _, 2, _, _, _, 1, _, _, _, 3, _,
                _, _, _, _, _, _, 2, _, _, _, _, _, _, _, 3, _,
                _, 2, _, _, _, 2, _, _, _, 2, _, _, _, 3, _, _,
                _, 2, _, _, _, 2, _, _, _, 3, _, _, _, 3, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, 2, _, _, _, 2, _, _, _, 2, _, _, _, 3, _, _,
                _, 2, _, _, _, 2, _, _, _, 3, _, _, _, 3, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
            };

            cycles = new int[] {
                _, _, _, _, _, _, 5, _, _, _, 2, _, _, _, 6, _,
                _, _, _, _, _, _, 6, _, _, _, _, _, _, _, 7, _,
                _, 6, _, _, _, 3, _, _, _, 2, _, _, _, 4, _, _,
                _, 5, _, _, _, 4, _, _, _, 4, _, _, _, 4, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, 6, _, _, _, 3, _, _, _, 2, _, _, _, 4, _, _,
                _, 5, _, _, _, 4, _, _, _, 4, _, _, _, 4, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
                _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _,
            };
        }

        public bool HaveInstruction(ushort opcode) => instruction[opcode] != null;

        public int Execute(ushort opcode) {
            mode = addressing[opcode];
            var tick = cycles[opcode];
            tick += instruction[opcode]();
            r.PC += bytes[opcode];
            return tick;
        }

        #region Instructions

        /// <summary> Add with Carry </summary>
        private int ADC() {

            var result = r.A + M + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            return PageCrossed ? 1 : 0;
        }
        /// <summary> Logical AND </summary>
        private int AND() {

            var result = r.A & M;

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            return PageCrossed ? 1 : 0;
        }
        /// <summary> Arithmetic Shift Left </summary>
        private int ASL() {

            var result = M << 1;

            r.PS.UpdateCarry(result);

            M = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            return 0;
        }
        #endregion
    }
}