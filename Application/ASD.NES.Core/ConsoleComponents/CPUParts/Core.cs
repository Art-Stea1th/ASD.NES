using System;
using OldCode;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    /// <summary> Emulation NMOS 6502 component of the CPU RP2A03 (Ricoh Processor 2A03) </summary>
    internal sealed class Core {

        private readonly Registers r;
        private readonly OldMemoryBus bus = OldMemoryBus.Instance;
        private readonly Func<int>[] instruction;

        private int ticks;

        public Core(Registers registers) {
            r = registers;
            instruction = new Func<int>[] {
                null,    null,    null, null, null, null,    null, null, null, null,    null, null, null, null,    null, null,
                null,    null,    null, null, null, null,    null, null, null, null,    null, null, null, null,    null, null,
                null,    AND_INX, null, null, null, AND_ZPG, null, null, null, AND_IMM, null, null, null, AND_ABS, null, null,
                null,    AND_INY, null, null, null, AND_ZPX, null, null, null, AND_ABY, null, null, null, AND_ABX, null, null,
                null,    null,    null, null, null, null,    null, null, null, null,    null, null, null, null,    null, null,
                null,    null,    null, null, null, null,    null, null, null, null,    null, null, null, null,    null, null,
                null,    ADC_INX, null, null, null, ADC_ZPG, null, null, null, ADC_IMM, null, null, null, ADC_ABS, null, null,
                null,    ADC_INY, null, null, null, ADC_ZPX, null, null, null, ADC_ABY, null, null, null, ADC_ABX, null, null,
                null,    null,    null, null, null, null,    null, null, null, null,    null, null, null, null,    null, null,
                null,    null,    null, null, null, null,    null, null, null, null,    null, null, null, null,    null, null,
                null,    null,    null, null, null, null,    null, null, null, null,    null, null, null, null,    null, null,
                null,    null,    null, null, null, null,    null, null, null, null,    null, null, null, null,    null, null,
                null,    null,    null, null, null, null,    null, null, null, null,    null, null, null, null,    null, null,
                null,    null,    null, null, null, null,    null, null, null, null,    null, null, null, null,    null, null,
                null,    null,    null, null, null, null,    null, null, null, null,    null, null, null, null,    null, null,
                null,    null,    null, null, null, null,    null, null, null, null,    null, null, null, null,    null, null,
            };
        }

        public bool HaveInstruction(ushort opcode) => instruction[opcode] != null;

        public int Execute(ushort opcode) {
            ticks = 0;
            return instruction[opcode]();
        }

        #region Addressing Modes
        public byte Immediate(byte argOne) {
            return argOne;
        }
        public byte ZeroPage(ushort argOne) {
            return bus.Read(argOne);
        }
        public byte ZeroPageX(ushort argOne) {
            return bus.Read(argOne + r.X);
        }
        public byte ZeroPageY(ushort argOne) {
            return bus.Read(argOne + r.Y);
        }
        public byte Absolute(byte argOne, byte argTwo) {
            return bus.Read(MakeAddress(argOne, argTwo));
        }
        public byte AbsoluteX(byte argOne, byte argTwo) {
            return bus.Read(MakeAddress(argOne, argTwo) + r.X);
        }
        public byte AbsoluteY(byte argOne, byte argTwo) {
            return bus.Read(MakeAddress(argOne, argTwo) + r.Y);
        }
        public byte IndirectX(byte argOne) {
            return bus.Read(ReadX2(bus.Read(argOne + r.X)));
        }
        public byte IndirectY(byte argOne) {
            return bus.Read(ReadX2(argOne) + r.Y);
        }
        #endregion
        #region Page Crossed checks
        private bool AbsoluteXCrossed(byte argOne, byte argTwo) {
            if ((MakeAddress(argOne, argTwo) & 0xFF00) == ((MakeAddress(argOne, argTwo) + r.X) & 0xFF00)) {
                return false;
            }
            return true;
        }
        private bool AbsoluteYCrossed(byte argOne, byte argTwo) {
            if ((MakeAddress(argOne, argTwo) & 0xFF00) == ((MakeAddress(argOne, argTwo) + r.Y) & 0xFF00)) {
                return false;
            }
            return true;
        }
        private bool IndirectYCrossed(byte argOne) {
            if ((ReadX2(argOne) & 0xFF00) == ((ReadX2(argOne) + r.Y) & 0xFF00)) {
                return false;
            }
            return true;
        }
        #endregion
        #region Instructions
        #region ADC: Add memory to accumulator with carry
        private int ADC_IMM() { // 0x69 / pc2 / t2

            var memory = Immediate(ArgOne);

            var result = r.A + memory + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 2; var ticks = 2;
            return ticks;
        }
        private int ADC_ZPG() { // 0x65 / pc2 / t3

            var memory = ZeroPage(ArgOne);

            var result = r.A + memory + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 2; var ticks = 3;
            return ticks;
        }
        private int ADC_ZPX() { // 0x75 / pc2 / t4

            var memory = ZeroPageX(ArgOne);

            var result = r.A + memory + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 2; var ticks = 4;
            return ticks;
        }
        private int ADC_ABS() { // 0x6D / pc3 / t4

            var memory = Absolute(ArgOne, ArgTwo);

            var result = r.A + memory + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 3; var ticks = 4;
            return ticks;
        }
        private int ADC_ABX() { // 0x7D / pc3 / t4*

            var memory = AbsoluteX(ArgOne, ArgTwo);

            var result = r.A + memory + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 3; var ticks = 4;
            //if (AbsoluteXCrossed(ArgOne, ArgTwo)) { ticks += 1;
            //}
            ticks += SamePage(ArgOne, (ushort)(MakeAddress(ArgOne, ArgTwo) + r.X)) ? 0 : 1;
            return ticks;
        }
        private int ADC_ABY() { // 0x79 / pc3 / t4*

            var memory = AbsoluteY(ArgOne, ArgTwo);

            var result = r.A + memory + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 3; var ticks = 4;
            ticks += SamePage(ArgOne, (ushort)(MakeAddress(ArgOne, ArgTwo) + r.Y)) ? 0 : 1;
            return ticks;
        }
        private int ADC_INX() { // 0x61 / pc2 / t6

            var memory = IndirectX(ArgOne);

            var result = r.A + memory + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 2; var ticks = 6;
            return ticks;
        }
        private int ADC_INY() { // 0x71 / pc2 / t5*

            var memory = IndirectY(ArgOne);

            var result = r.A + memory + r.PS.C;

            r.PS.UpdateOverflow(result);
            r.PS.UpdateCarry(result);

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 2; var ticks = 5;
            ticks += SamePage(ReadX2(ArgOne), ReadX2((ushort)(ArgOne + r.Y))) ? 0 : 1;
            return ticks;
        }
        #endregion
        #region AND: "AND" memory with accumulator
        private int AND_IMM() { // 0x29 / pc2 / t2

            var memory = Immediate(ArgOne);

            var result = r.A & memory;

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 2; var ticks = 2;
            return ticks;
        }
        private int AND_ZPG() { // 0x25 / pc2 / t3

            var memory = ZeroPage(ArgOne);

            var result = r.A & memory;

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 2; var ticks = 3;
            return ticks;
        }
        private int AND_ZPX() { // 0x35 / pc2 / t4

            var memory = ZeroPageX(ArgOne);

            var result = r.A & memory;

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 2; var ticks = 4;
            return ticks;
        }
        private int AND_ABS() { // 0x2D / pc3 / t4

            var memory = Absolute(ArgOne, ArgTwo);

            var result = r.A & memory;

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 3; var ticks = 4;
            return ticks;
        }
        private int AND_ABX() { // 0x3D / pc3 / t4*

            var memory = AbsoluteX(ArgOne, ArgTwo);

            var result = r.A & memory;

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 3; var ticks = 4;
            ticks += SamePage(ArgOne, (ushort)(MakeAddress(ArgOne, ArgTwo) + r.X)) ? 0 : 1;
            return ticks;
        }
        private int AND_ABY() { // 0x39 / pc3 / t4*

            var memory = AbsoluteY(ArgOne, ArgTwo);

            var result = r.A & memory;

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 3; var ticks = 4;
            ticks += SamePage(ArgOne, (ushort)(MakeAddress(ArgOne, ArgTwo) + r.X)) ? 0 : 1;
            return ticks;
        }
        private int AND_INX() { // 0x21 / pc2 / t6

            var memory = IndirectX(ArgOne);

            var result = r.A & memory;

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 3; var ticks = 6;
            return ticks;
        }
        private int AND_INY2() { // 0x31 / pc2 / t5*

            var memory = IndirectY(ArgOne);

            var result = r.A & memory;

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 3; var ticks = 5;
            ticks += SamePage(ReadX2(ArgOne), ReadX2((ushort)(ArgOne + r.Y))) ? 0 : 1;
            return ticks;
        }

        private int AND_INY() {
            //ushort addressWithoutY = ReadX2(ArgOne);
            //ushort addressWithY = (ushort)(addressWithoutY + r.Y);
            //byte arg = bus.Read(addressWithY);

            var memory = IndirectY((byte)(ArgOne/* + r.Y*/));

            var result = r.A & memory;

            r.A = (byte)(result);

            r.PS.UpdateSigned(r.A);
            r.PS.UpdateZero(r.A);

            r.PC += 2; var ticks = SamePage((ushort)(ReadX2PW(ArgOne) + r.Y), ReadX2PW(ArgOne)) ? 5 : 6;
            return ticks;
        }

        public ushort ReadX2PW(ushort address) {

            var hiAddress = (address & 0x00FF) == 0x00FF ? address & 0xFF00 : address + 1;

            var hiByte = bus.Read((ushort)hiAddress);
            var loByte = bus.Read(address);

            return (ushort)((hiByte << 8) | loByte);
        }
        #endregion
        #endregion
        #region Helpers
        private byte ArgOne => bus[r.PC + 1];
        private byte ArgTwo => bus[r.PC + 2];

        public ushort MakeAddress(byte argOne, byte argTwo)
            => (ushort)((argTwo << 8) & argOne);
        public bool SamePage(ushort addressOne, ushort addressTwo) {
            return (addressOne & 0xFF00) == (addressTwo & 0xFF00);
        }
        public ushort ReadX2(ushort address) {
            return (ushort)((bus.Read((ushort)(address + 1)) << 8) | bus.Read(address));
        }
        #endregion
    }
}