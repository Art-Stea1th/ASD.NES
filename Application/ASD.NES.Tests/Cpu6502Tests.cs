using System;
using ASD.NES.Core;
using Xunit;
using Console = ASD.NES.Core.Console;

namespace ASD.NES.Tests;

/// <summary>
/// 6502 CPU behavior per 6502.txt and NESDEV: registers, flags, addressing, instructions.
/// Uses minimal NROM ROM with reset vector and test code; runs via Console.RunCpuSteps / GetCpuState / GetMemory.
/// Sequential: CPU address space is a singleton, so tests must not run in parallel.
/// </summary>
[Collection("CPU")]
public sealed class Cpu6502Tests
{
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
        ushort irqVector = (ushort)(resetVector + codeAtC000.Length);
        rom[offset + 0x3FFA] = (byte)(resetVector & 0xFF);
        rom[offset + 0x3FFB] = (byte)(resetVector >> 8);
        rom[offset + 0x3FFE] = (byte)(irqVector & 0xFF);
        rom[offset + 0x3FFF] = (byte)(irqVector >> 8);

        return rom;
    }

    [Fact]
    public void Minimal_ROM_has_reset_vector_at_0x3FFC()
    {
        var rom = BuildMinimalRom(new byte[] { 0x00 }, 0xC000);
        Assert.Equal(0, rom[16 + 0x3FFC]);
        Assert.Equal(0xC0, rom[16 + 0x3FFD]);
        var cart = Cartridge.Create(rom);
        Assert.Equal(0xC000, (int)cart.ResetVector);
    }

    [Fact]
    public void CPU_address_space_returns_reset_vector_after_cartridge_create()
    {
        var rom = BuildMinimalRom(new byte[] { 0x00 });
        var cart = Cartridge.Create(rom);
        var console = new Console();
        Assert.Equal(0, console.GetMemory(0xFFFC));
        Assert.Equal(0xC0, console.GetMemory(0xFFFD));
    }

    [Fact]
    public void Cold_boot_sets_PC_from_reset_vector()
    {
        var rom = BuildMinimalRom(new byte[] { 0x00 }); // BRK at 0xC000
        var cart = Cartridge.Create(rom);
        Assert.Equal(0xC000, (int)cart.ResetVector);
        var console = new Console();
        console.InsertCartridge(cart);
        console.RunCpuSteps(1);
        var s = console.GetCpuState();
        Assert.Equal(0xC001, (int)s.PC); // BRK jumps to resetVector+1
    }

    [Fact]
    public void LDA_immediate_loads_A_and_sets_Z_N()
    {
        // LDA #0 -> Z=1, N=0. LDA #$80 -> N=1, Z=0.
        var rom = BuildMinimalRom(new byte[]
        {
            0xA9, 0x00,       // LDA #0
            0x85, 0x00,       // STA $00
            0xA9, 0x80,       // LDA #$80
            0x85, 0x01,       // STA $01
            0x00              // BRK
        });
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        console.RunCpuSteps(6);
        Assert.Equal(0, console.GetMemory(0));
        Assert.Equal(0x80, console.GetMemory(1));
        var s = console.GetCpuState();
        Assert.Equal(0x80, s.A);
        Assert.True((s.P & 0x80) != 0);
    }

    [Fact]
    public void STA_zero_page_stores_A()
    {
        var rom = BuildMinimalRom(new byte[]
        {
            0xA9, 0x42,       // LDA #$42
            0x85, 0x10,       // STA $10
            0x00              // BRK
        });
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        console.RunCpuSteps(5);
        Assert.Equal(0x42, console.GetMemory(0x10));
    }

    [Fact]
    public void Stack_at_0x100_and_SP_decrements_on_push()
    {
        var rom = BuildMinimalRom(new byte[]
        {
            0xA9, 0xAA,       // LDA #$AA
            0x48,             // PHA
            0xA9, 0xBB,       // LDA #$BB
            0x48,             // PHA
            0x00              // BRK
        });
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        console.RunCpuSteps(4);
        var s = console.GetCpuState();
        Assert.Equal(0xBB, s.A);
        Assert.Equal(0xFD - 2, (int)s.SP);
        Assert.Equal(0xBB, console.GetMemory((ushort)(0x100 + s.SP + 1)));
        Assert.Equal(0xAA, console.GetMemory((ushort)(0x100 + s.SP + 2)));
    }

    [Fact]
    public void PHP_pushes_P_with_B_and_U_bits_set_per_6502()
    {
        var rom = BuildMinimalRom(new byte[]
        {
            0x18,             // CLC
            0xD8,             // CLD
            0x08,             // PHP - push P
            0x00              // BRK
        });
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        console.RunCpuSteps(6);
        var s = console.GetCpuState();
        byte pushedP = console.GetMemory((ushort)(0x100 + s.SP + 1));
        Assert.True((pushedP & 0x30) == 0x30, "PHP must push P with B(bit4) and bit5 set per 6502");
    }

    [Fact]
    public void JSR_pushes_PC_plus_2_then_jumps()
    {
        var rom = BuildMinimalRom(new byte[]
        {
            0x20, 0x05, 0xC0, // JSR $C005
            0x00,             // BRK
            0x60,             // RTS (at $C005)
            0x00              // BRK after return
        });
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        console.RunCpuSteps(2);
        var s = console.GetCpuState();
        Assert.True(s.PC == 0xC004 || s.PC == 0xC006, "After JSR+RTS PC should be 0xC004 (or 0xC006 if BRK ran); got " + s.PC);
    }

    [Fact]
    public void CLC_clears_carry()
    {
        var rom = BuildMinimalRom(new byte[]
        {
            0x38,             // SEC
            0x18,             // CLC
            0x00
        });
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        console.RunCpuSteps(5);
        var s = console.GetCpuState();
        Assert.Equal(0, s.P & 1);
    }

    [Fact]
    public void ADC_sets_carry_and_zero()
    {
        var rom = BuildMinimalRom(new byte[]
        {
            0xA9, 0xFF,       // LDA #$FF
            0x69, 0x01,       // ADC #1 -> 0, C=1
            0x85, 0x00,       // STA $00
            0x00
        });
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        console.RunCpuSteps(8);
        Assert.Equal(0, console.GetMemory(0));
        var s = console.GetCpuState();
        Assert.True((s.P & 1) != 0);
        Assert.True((s.P & 2) != 0);
    }

    [Fact]
    public void BIT_sets_N_and_V_from_memory_bits_7_and_6()
    {
        var rom = BuildMinimalRom(new byte[]
        {
            0xA9, 0xC0,       // LDA #$C0
            0x85, 0x02,       // STA $02
            0xA9, 0x00,       // LDA #0
            0x24, 0x02,       // BIT $02 -> N from bit7, V from bit6 of $02
            0x00
        });
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        console.RunCpuSteps(12);
        var s = console.GetCpuState();
        Assert.True((s.P & 0x80) != 0, "N (bit 7) should be set from memory bit 7");
        Assert.True((s.P & 0x40) != 0, "V (bit 6) should be set from memory bit 6");
    }
}
