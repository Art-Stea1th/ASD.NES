using System;
using System.Collections.Generic;
using ASD.NES.Core;
using Xunit;
using Console = ASD.NES.Core.Console;

namespace ASD.NES.Tests;

/// <summary>
/// Parameterized tests: every 6502 instruction and addressing mode per 6502.txt.
/// Each case runs one instruction (with optional setup), then asserts registers and memory.
/// </summary>
[Collection("CPU")]
public sealed class CpuInstructionTests
{
    public sealed class InstructionTestCase
    {
        public string Name { get; set; } = "";
        public byte[] Code { get; set; } = Array.Empty<byte>();
        public int Steps { get; set; }
        public List<(ushort Address, byte Value)>? Setup { get; set; }
        public byte? ExpectA { get; set; }
        public byte? ExpectX { get; set; }
        public byte? ExpectY { get; set; }
        public byte? ExpectP { get; set; }
        public List<(ushort Address, byte Value)>? MemoryChecks { get; set; }
    }

    private static byte[] BuildMinimalRom(byte[] codeAtC000, ushort resetVector = 0xC000)
    {
        const int HeaderLen = 16;
        const int PrgLen = 0x4000;
        const int ChrLen = 0x2000;
        var rom = new byte[HeaderLen + PrgLen + ChrLen];
        rom[0] = 0x4E;
        rom[1] = 0x45;
        rom[2] = 0x53;
        rom[3] = 0x1A;
        rom[4] = 1;
        rom[5] = 1;
        rom[6] = 0;
        rom[7] = 0;
        int offset = HeaderLen;
        for (int i = 0; i < codeAtC000.Length && i < 0x4000; i++) {
            rom[offset + i] = codeAtC000[i];
        }
        for (int i = 0x3FFC; i < 0x4000 && (i - 0x3FFC) < 2; i++) {
            rom[offset + i] = (byte)((i == 0x3FFC) ? (resetVector & 0xFF) : (resetVector >> 8));
        }
        ushort irqVec = (ushort)(resetVector + codeAtC000.Length);
        rom[offset + 0x3FFA] = (byte)(resetVector & 0xFF);
        rom[offset + 0x3FFB] = (byte)(resetVector >> 8);
        rom[offset + 0x3FFE] = (byte)(irqVec & 0xFF);
        rom[offset + 0x3FFF] = (byte)(irqVec >> 8);
        return rom;
    }

    [Theory]
    [MemberData(nameof(AllInstructionCases))]
    public void Each_instruction_addressing_mode_executes_correctly(InstructionTestCase t)
    {
        var cart = Cartridge.Create(BuildMinimalRom(t.Code));
        var console = new Console();
        console.InsertCartridge(cart);
        if (t.Setup != null) {
            foreach (var (addr, value) in t.Setup) {
                console.SetMemory(addr, value);
            }
        }
        console.RunCpuSteps(t.Steps);
        var s = console.GetCpuState();
        if (t.ExpectA.HasValue) {
            Assert.True(s.A == t.ExpectA.Value, $"{t.Name}: expected A=0x{t.ExpectA.Value:X2}, got 0x{s.A:X2}");
        }
        if (t.ExpectX.HasValue) {
            Assert.True(s.X == t.ExpectX.Value, $"{t.Name}: expected X=0x{t.ExpectX.Value:X2}, got 0x{s.X:X2}");
        }
        if (t.ExpectY.HasValue) {
            Assert.True(s.Y == t.ExpectY.Value, $"{t.Name}: expected Y=0x{t.ExpectY.Value:X2}, got 0x{s.Y:X2}");
        }
        if (t.ExpectP.HasValue) {
            Assert.True(s.P == t.ExpectP.Value, $"{t.Name}: expected P=0x{t.ExpectP.Value:X2}, got 0x{s.P:X2}");
        }
        if (t.MemoryChecks != null) {
            foreach (var (addr, value) in t.MemoryChecks) {
                var actual = console.GetMemory(addr);
                Assert.True(actual == value, $"{t.Name}: at 0x{addr:X4} expected 0x{value:X2}, got 0x{actual:X2}");
            }
        }
    }

    public static IEnumerable<object[]> AllInstructionCases()
    {
        foreach (var t in AllCases()) {
            yield return new object[] { t };
        }
    }

    private static List<InstructionTestCase> AllCases()
    {
        var list = new List<InstructionTestCase>();
        const byte BaseP = 0x34;
        void Add(InstructionTestCase t) => list.Add(t);

        // --- LDA (Load Accumulator) ---
        Add(new InstructionTestCase { Name = "LDA_imm", Code = new byte[] { 0xA9, 0x42, 0x00 }, Steps = 2, ExpectA = 0x42 });
        Add(new InstructionTestCase { Name = "LDA_zpg", Code = new byte[] { 0xA5, 0x20, 0x00 }, Steps = 2, Setup = new List<(ushort, byte)> { (0x20, 0xAB) }, ExpectA = 0xAB });
        Add(new InstructionTestCase { Name = "LDA_zpx", Code = new byte[] { 0xA2, 0x01, 0xB5, 0x20, 0x00 }, Steps = 3, Setup = new List<(ushort, byte)> { (0x21, 0xCD) }, ExpectX = 1, ExpectA = 0xCD });
        Add(new InstructionTestCase { Name = "LDA_abs", Code = new byte[] { 0xAD, 0x10, 0xC0, 0x00 }.PadTo(0x11).WithPatch(0x10, 0x77), Steps = 2, ExpectA = 0x77 });
        Add(new InstructionTestCase { Name = "LDA_abx", Code = new byte[] { 0xA2, 0x02, 0xBD, 0x0E, 0xC0, 0x00 }.PadTo(0x11).WithPatch(0x10, 0x11), Steps = 3, ExpectX = 2, ExpectA = 0x11 });
        Add(new InstructionTestCase { Name = "LDA_aby", Code = new byte[] { 0xA0, 0x02, 0xB9, 0x0E, 0xC0, 0x00 }.PadTo(0x11).WithPatch(0x10, 0x22), Steps = 3, ExpectY = 2, ExpectA = 0x22 });
        Add(new InstructionTestCase { Name = "LDA_idx", Code = new byte[] { 0xA1, 0x20, 0x00, 0xEA, 0xEA, 0xEA, 0x33 }, Steps = 2, Setup = new List<(ushort, byte)> { (0x20, 0x06), (0x21, 0xC0) }, ExpectX = 0, ExpectA = 0x33 });
        Add(new InstructionTestCase { Name = "LDA_idy", Code = new byte[] { 0xA0, 0x01, 0xB1, 0x40, 0x00, 0xEA, 0x44 }, Steps = 3, Setup = new List<(ushort, byte)> { (0x40, 0x05), (0x41, 0xC0) }, ExpectY = 1, ExpectA = 0x44 });

        // --- STA (Store Accumulator) ---
        Add(new InstructionTestCase { Name = "STA_zpg", Code = new byte[] { 0xA9, 0x55, 0x85, 0x30, 0x00 }, Steps = 4, MemoryChecks = new List<(ushort, byte)> { (0x30, 0x55) } });
        Add(new InstructionTestCase { Name = "STA_zpx", Code = new byte[] { 0xA9, 0x66, 0xA2, 0x02, 0x95, 0x30, 0x00 }, Steps = 6, MemoryChecks = new List<(ushort, byte)> { (0x32, 0x66) } });
        Add(new InstructionTestCase { Name = "STA_abs", Code = new byte[] { 0xA9, 0x77, 0x8D, 0x00, 0x02, 0x00 }, Steps = 4, MemoryChecks = new List<(ushort, byte)> { (0x0200, 0x77) } });
        Add(new InstructionTestCase { Name = "STA_abx", Code = new byte[] { 0xA9, 0x88, 0xA2, 0x01, 0x9D, 0xFF, 0x01, 0x00 }, Steps = 6, MemoryChecks = new List<(ushort, byte)> { (0x0200, 0x88) } });
        Add(new InstructionTestCase { Name = "STA_aby", Code = new byte[] { 0xA9, 0x99, 0xA0, 0x01, 0x99, 0xFF, 0x01, 0x00 }, Steps = 6, MemoryChecks = new List<(ushort, byte)> { (0x0200, 0x99) } });
        Add(new InstructionTestCase { Name = "STA_idx", Code = new byte[] { 0xA9, 0xAA, 0xA2, 0x00, 0x81, 0x40, 0x00 }, Steps = 4, Setup = new List<(ushort, byte)> { (0x40, 0x00), (0x41, 0x02) }, MemoryChecks = new List<(ushort, byte)> { (0x0200, 0xAA) } });
        Add(new InstructionTestCase { Name = "STA_idy", Code = new byte[] { 0xA9, 0xBB, 0xA0, 0x01, 0x91, 0x40, 0x00 }, Steps = 4, Setup = new List<(ushort, byte)> { (0x40, 0xFF), (0x41, 0x01) }, MemoryChecks = new List<(ushort, byte)> { (0x0200, 0xBB) } });

        // --- LDX ---
        Add(new InstructionTestCase { Name = "LDX_imm", Code = new byte[] { 0xA2, 0x42, 0x00 }, Steps = 2, ExpectX = 0x42 });
        Add(new InstructionTestCase { Name = "LDX_zpg", Code = new byte[] { 0xA6, 0x20, 0x00 }, Steps = 2, Setup = new List<(ushort, byte)> { (0x20, 0xAB) }, ExpectX = 0xAB });
        Add(new InstructionTestCase { Name = "LDX_zpy", Code = new byte[] { 0xA0, 0x01, 0xB6, 0x20, 0x00 }, Steps = 3, Setup = new List<(ushort, byte)> { (0x21, 0xCD) }, ExpectY = 1, ExpectX = 0xCD });
        Add(new InstructionTestCase { Name = "LDX_abs", Code = new byte[] { 0xAE, 0x10, 0xC0, 0x00 }.PadTo(0x11).WithPatch(0x10, 0x77), Steps = 2, ExpectX = 0x77 });
        Add(new InstructionTestCase { Name = "LDX_aby", Code = new byte[] { 0xA0, 0x02, 0xBE, 0x0E, 0xC0, 0x00 }.PadTo(0x11).WithPatch(0x10, 0x11), Steps = 3, ExpectY = 2, ExpectX = 0x11 });

        // --- LDY ---
        Add(new InstructionTestCase { Name = "LDY_imm", Code = new byte[] { 0xA0, 0x42, 0x00 }, Steps = 2, ExpectY = 0x42 });
        Add(new InstructionTestCase { Name = "LDY_zpg", Code = new byte[] { 0xA4, 0x20, 0x00 }, Steps = 2, Setup = new List<(ushort, byte)> { (0x20, 0xAB) }, ExpectY = 0xAB });
        Add(new InstructionTestCase { Name = "LDY_zpx", Code = new byte[] { 0xA2, 0x01, 0xB4, 0x20, 0x00 }, Steps = 3, Setup = new List<(ushort, byte)> { (0x21, 0xCD) }, ExpectX = 1, ExpectY = 0xCD });
        Add(new InstructionTestCase { Name = "LDY_abs", Code = new byte[] { 0xAC, 0x10, 0xC0, 0x00 }.PadTo(0x11).WithPatch(0x10, 0x77), Steps = 2, ExpectY = 0x77 });
        Add(new InstructionTestCase { Name = "LDY_abx", Code = new byte[] { 0xA2, 0x02, 0xBC, 0x0E, 0xC0, 0x00 }.PadTo(0x11).WithPatch(0x10, 0x22), Steps = 3, ExpectX = 2, ExpectY = 0x22 });

        // --- STX / STY ---
        Add(new InstructionTestCase { Name = "STX_zpg", Code = new byte[] { 0xA2, 0x55, 0x86, 0x30, 0x00 }, Steps = 4, MemoryChecks = new List<(ushort, byte)> { (0x30, 0x55) } });
        Add(new InstructionTestCase { Name = "STX_zpy", Code = new byte[] { 0xA2, 0x66, 0xA0, 0x02, 0x96, 0x30, 0x00 }, Steps = 6, MemoryChecks = new List<(ushort, byte)> { (0x32, 0x66) } });
        Add(new InstructionTestCase { Name = "STX_abs", Code = new byte[] { 0xA2, 0x77, 0x8E, 0x00, 0x02, 0x00 }, Steps = 4, MemoryChecks = new List<(ushort, byte)> { (0x0200, 0x77) } });
        Add(new InstructionTestCase { Name = "STY_zpg", Code = new byte[] { 0xA0, 0x55, 0x84, 0x30, 0x00 }, Steps = 4, MemoryChecks = new List<(ushort, byte)> { (0x30, 0x55) } });
        Add(new InstructionTestCase { Name = "STY_zpx", Code = new byte[] { 0xA0, 0x66, 0xA2, 0x02, 0x94, 0x30, 0x00 }, Steps = 6, MemoryChecks = new List<(ushort, byte)> { (0x32, 0x66) } });
        Add(new InstructionTestCase { Name = "STY_abs", Code = new byte[] { 0xA0, 0x77, 0x8C, 0x00, 0x02, 0x00 }, Steps = 4, MemoryChecks = new List<(ushort, byte)> { (0x0200, 0x77) } });

        // --- Transfers (implied) ---
        Add(new InstructionTestCase { Name = "TAX", Code = new byte[] { 0xA9, 0xAB, 0xAA, 0x00 }, Steps = 3, ExpectA = 0xAB, ExpectX = 0xAB });
        Add(new InstructionTestCase { Name = "TAY", Code = new byte[] { 0xA9, 0xCD, 0xA8, 0x00 }, Steps = 3, ExpectA = 0xCD, ExpectY = 0xCD });
        Add(new InstructionTestCase { Name = "TXA", Code = new byte[] { 0xA2, 0x12, 0x8A, 0x00 }, Steps = 3, ExpectA = 0x12, ExpectX = 0x12 });
        Add(new InstructionTestCase { Name = "TYA", Code = new byte[] { 0xA0, 0x34, 0x98, 0x00 }, Steps = 3, ExpectA = 0x34, ExpectY = 0x34 });
        Add(new InstructionTestCase { Name = "TXS", Code = new byte[] { 0xA2, 0xFE, 0x9A, 0x00 }, Steps = 3, ExpectX = 0xFE });
        Add(new InstructionTestCase { Name = "TSX", Code = new byte[] { 0xA2, 0xDD, 0x9A, 0xBA, 0x00 }, Steps = 4, ExpectX = 0xDD });

        // --- INC / DEC (memory) ---
        Add(new InstructionTestCase { Name = "INC_zpg", Code = new byte[] { 0xA9, 0x40, 0x85, 0x20, 0xE6, 0x20, 0x00 }, Steps = 6, MemoryChecks = new List<(ushort, byte)> { (0x20, 0x41) } });
        Add(new InstructionTestCase { Name = "INC_zpx", Code = new byte[] { 0xA9, 0x50, 0x85, 0x21, 0xA2, 0x01, 0xF6, 0x20, 0x00 }, Steps = 8, MemoryChecks = new List<(ushort, byte)> { (0x21, 0x51) } });
        Add(new InstructionTestCase { Name = "INC_abs", Code = new byte[] { 0xA9, 0x60, 0x8D, 0x00, 0x02, 0xEE, 0x00, 0x02, 0x00 }, Steps = 8, MemoryChecks = new List<(ushort, byte)> { (0x0200, 0x61) } });
        Add(new InstructionTestCase { Name = "DEC_zpg", Code = new byte[] { 0xA9, 0x02, 0x85, 0x20, 0xC6, 0x20, 0x00 }, Steps = 6, MemoryChecks = new List<(ushort, byte)> { (0x20, 0x01) } });
        Add(new InstructionTestCase { Name = "DEC_zpx", Code = new byte[] { 0xA9, 0x03, 0x85, 0x21, 0xA2, 0x01, 0xD6, 0x20, 0x00 }, Steps = 8, MemoryChecks = new List<(ushort, byte)> { (0x21, 0x02) } });
        Add(new InstructionTestCase { Name = "DEC_abs", Code = new byte[] { 0xA9, 0x10, 0x8D, 0x00, 0x02, 0xCE, 0x00, 0x02, 0x00 }, Steps = 8, MemoryChecks = new List<(ushort, byte)> { (0x0200, 0x0F) } });

        // --- INX / INY / DEX / DEY ---
        Add(new InstructionTestCase { Name = "INX", Code = new byte[] { 0xA2, 0x41, 0xE8, 0x00 }, Steps = 3, ExpectX = 0x42 });
        Add(new InstructionTestCase { Name = "INY", Code = new byte[] { 0xA0, 0x41, 0xC8, 0x00 }, Steps = 3, ExpectY = 0x42 });
        Add(new InstructionTestCase { Name = "DEX", Code = new byte[] { 0xA2, 0x01, 0xCA, 0x00 }, Steps = 2, ExpectX = 0, ExpectP = (byte)(0x02 | BaseP) });
        Add(new InstructionTestCase { Name = "DEY", Code = new byte[] { 0xA0, 0x01, 0x88, 0x00 }, Steps = 2, ExpectY = 0, ExpectP = (byte)(0x02 | BaseP) });

        // --- ADC (with C=0) ---
        Add(new InstructionTestCase { Name = "ADC_imm", Code = new byte[] { 0x18, 0xA9, 0x10, 0x69, 0x20, 0x00 }, Steps = 6, ExpectA = 0x30 });
        Add(new InstructionTestCase { Name = "ADC_zpg", Code = new byte[] { 0x18, 0xA5, 0x20, 0x00 }, Steps = 2, Setup = new List<(ushort, byte)> { (0x20, 0x30) }, ExpectA = 0x30 });
        Add(new InstructionTestCase { Name = "ADC_abs", Code = new byte[] { 0x18, 0xA9, 0x00, 0x6D, 0x10, 0xC0, 0x00 }.PadTo(0x11).WithPatch(0x10, 0x11), Steps = 4, ExpectA = 0x11 });

        // --- SBC (A - M - ~C) ---
        Add(new InstructionTestCase { Name = "SBC_imm", Code = new byte[] { 0x38, 0xA9, 0x50, 0xE9, 0x10, 0x00 }, Steps = 6, ExpectA = 0x40 });
        Add(new InstructionTestCase { Name = "SBC_zpg", Code = new byte[] { 0x38, 0xA9, 0x30, 0xE5, 0x20, 0x00 }, Steps = 4, Setup = new List<(ushort, byte)> { (0x20, 0x10) }, ExpectA = 0x20 });

        // --- AND / ORA / EOR ---
        Add(new InstructionTestCase { Name = "AND_imm", Code = new byte[] { 0xA9, 0xFF, 0x29, 0x0F, 0x00 }, Steps = 4, ExpectA = 0x0F });
        Add(new InstructionTestCase { Name = "ORA_imm", Code = new byte[] { 0xA9, 0x0F, 0x09, 0xF0, 0x00 }, Steps = 4, ExpectA = 0xFF });
        Add(new InstructionTestCase { Name = "EOR_imm", Code = new byte[] { 0xA9, 0xFF, 0x49, 0x0F, 0x00 }, Steps = 4, ExpectA = 0xF0 });

        // --- ASL (Accumulator and memory) ---
        Add(new InstructionTestCase { Name = "ASL_acc", Code = new byte[] { 0xA9, 0x41, 0x0A, 0x00 }, Steps = 3, ExpectA = 0x82 });
        Add(new InstructionTestCase { Name = "ASL_zpg", Code = new byte[] { 0xA9, 0x40, 0x85, 0x20, 0x06, 0x20, 0x00 }, Steps = 6, MemoryChecks = new List<(ushort, byte)> { (0x20, 0x80) } });

        // --- LSR ---
        Add(new InstructionTestCase { Name = "LSR_acc", Code = new byte[] { 0xA9, 0x82, 0x4A, 0x00 }, Steps = 3, ExpectA = 0x41 });
        Add(new InstructionTestCase { Name = "LSR_zpg", Code = new byte[] { 0xA9, 0x80, 0x85, 0x20, 0x46, 0x20, 0x00 }, Steps = 6, MemoryChecks = new List<(ushort, byte)> { (0x20, 0x40) } });

        // --- ROL / ROR ---
        Add(new InstructionTestCase { Name = "ROL_acc", Code = new byte[] { 0x18, 0xA9, 0x81, 0x2A, 0x00 }, Steps = 4, ExpectA = 0x02, ExpectP = (byte)(0x01 | BaseP) });
        Add(new InstructionTestCase { Name = "ROR_acc", Code = new byte[] { 0x38, 0xA9, 0x01, 0x6A, 0x00 }, Steps = 3, ExpectA = 0x80, ExpectP = (byte)(0x01 | 0x80 | BaseP) });

        // --- CMP / CPX / CPY ---
        Add(new InstructionTestCase { Name = "CMP_imm", Code = new byte[] { 0xA9, 0x40, 0xC9, 0x40, 0x00 }, Steps = 2, ExpectA = 0x40, ExpectP = (byte)(0x03 | BaseP) });
        Add(new InstructionTestCase { Name = "CPX_imm", Code = new byte[] { 0xA2, 0x30, 0xE0, 0x30, 0x00 }, Steps = 2, ExpectX = 0x30, ExpectP = (byte)(0x03 | BaseP) });
        Add(new InstructionTestCase { Name = "CPY_imm", Code = new byte[] { 0xA0, 0x20, 0xC0, 0x20, 0x00 }, Steps = 2, ExpectY = 0x20, ExpectP = (byte)(0x03 | BaseP) });

        // --- BIT ---
        Add(new InstructionTestCase { Name = "BIT_zpg", Code = new byte[] { 0xA9, 0x00, 0x24, 0x20, 0x00 }, Steps = 2, Setup = new List<(ushort, byte)> { (0x20, 0xC0) }, ExpectA = 0, ExpectP = (byte)(0x80 | 0x40 | 0x02 | BaseP) });

        // --- Stack ---
        Add(new InstructionTestCase { Name = "PHA_PLA", Code = new byte[] { 0xA9, 0x77, 0x48, 0xA9, 0x00, 0x68, 0x00 }, Steps = 7, ExpectA = 0x77 });
        Add(new InstructionTestCase { Name = "PHP_PLP", Code = new byte[] { 0x18, 0x08, 0x38, 0x28, 0x00 }, Steps = 4, ExpectP = BaseP });

        // --- JMP ---
        Add(new InstructionTestCase { Name = "JMP_abs", Code = new byte[] { 0x4C, 0x06, 0xC0, 0x00, 0xEA, 0xEA, 0xA9, 0x99, 0x85, 0x00, 0x00 }, Steps = 4, MemoryChecks = new List<(ushort, byte)> { (0x00, 0x99) } });

        // --- NOP ---
        Add(new InstructionTestCase { Name = "NOP", Code = new byte[] { 0xEA, 0xA9, 0x11, 0x85, 0x00, 0x00 }, Steps = 4, MemoryChecks = new List<(ushort, byte)> { (0x00, 0x11) } });

        // --- Flag instructions ---
        Add(new InstructionTestCase { Name = "CLC", Code = new byte[] { 0x38, 0x18, 0x00 }, Steps = 2, ExpectP = BaseP });
        Add(new InstructionTestCase { Name = "SEC", Code = new byte[] { 0x38, 0x00 }, Steps = 1, ExpectP = (byte)(0x01 | BaseP) });
        Add(new InstructionTestCase { Name = "CLI", Code = new byte[] { 0x78, 0x58, 0x00 }, Steps = 3 });
        Add(new InstructionTestCase { Name = "SEI", Code = new byte[] { 0x78, 0x00 }, Steps = 2 });
        Add(new InstructionTestCase { Name = "CLV", Code = new byte[] { 0xB8, 0x00 }, Steps = 2 });
        Add(new InstructionTestCase { Name = "CLD", Code = new byte[] { 0xF8, 0xD8, 0x00 }, Steps = 3 });
        Add(new InstructionTestCase { Name = "SED", Code = new byte[] { 0xF8, 0x00 }, Steps = 2 });

        // --- Branches (relative): BEQ when Z=1, BNE when Z=0 ---
        Add(new InstructionTestCase { Name = "BEQ_taken", Code = new byte[] { 0xA9, 0x40, 0xC9, 0x40, 0xF0, 0x02, 0xA9, 0x00, 0xA9, 0x99, 0x85, 0x00, 0x00 }, Steps = 5, MemoryChecks = new List<(ushort, byte)> { (0x00, 0x99) } });
        Add(new InstructionTestCase { Name = "BNE_taken", Code = new byte[] { 0xA9, 0x01, 0xD0, 0x02, 0xA9, 0x00, 0xA9, 0x77, 0x85, 0x00, 0x00 }, Steps = 5, MemoryChecks = new List<(ushort, byte)> { (0x00, 0x77) } });

        // --- AND/ORA/EOR zero page ---
        Add(new InstructionTestCase { Name = "AND_zpg", Code = new byte[] { 0xA9, 0xFF, 0x25, 0x20, 0x00 }, Steps = 2, Setup = new List<(ushort, byte)> { (0x20, 0x0F) }, ExpectA = 0x0F });
        Add(new InstructionTestCase { Name = "ORA_zpg", Code = new byte[] { 0xA9, 0x0F, 0x05, 0x20, 0x00 }, Steps = 2, Setup = new List<(ushort, byte)> { (0x20, 0xF0) }, ExpectA = 0xFF });
        Add(new InstructionTestCase { Name = "EOR_zpg", Code = new byte[] { 0xA9, 0xFF, 0x45, 0x20, 0x00 }, Steps = 2, Setup = new List<(ushort, byte)> { (0x20, 0x0F) }, ExpectA = 0xF0 });

        return list;
    }
}

internal static class CpuInstructionTestsExtensions
{
    public static byte[] WithPatch(this byte[] code, int offset, byte value)
    {
        var copy = (byte[])code.Clone();
        if (offset >= 0 && offset < copy.Length) {
            copy[offset] = value;
        }
        return copy;
    }

    public static byte[] PadTo(this byte[] code, int length)
    {
        if (code.Length >= length) {
            return code;
        }
        var copy = new byte[length];
        Array.Copy(code, copy, code.Length);
        for (int i = code.Length; i < length; i++) {
            copy[i] = 0xEA;
        }
        return copy;
    }
}
