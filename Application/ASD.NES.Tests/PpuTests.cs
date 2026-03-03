using System;
using ASD.NES.Core;
using ASD.NES.Core.ConsoleComponents.PPUParts;
using Xunit;
using Console = ASD.NES.Core.Console;

namespace ASD.NES.Tests;

/// <summary>
/// PPU register and memory-map behavior per NES docs (nes.txt, NESDEV PPU).
/// CPU maps 0x2000-0x3FFF to PPU registers (mirrored every 8 bytes); write-only on hardware, emulator returns last written.
/// Nametable mirroring: Specification/Other/nestech.txt (G. Name Table Mirroring) and nes.txt — Vertical: ($2000,$2800) and ($2400,$2C00) same; Horizontal: ($2000,$2400) and ($2800,$2C00) same.
/// </summary>
[Collection("CPU")]
public sealed class PpuTests
{
    private static void SetPpuAddr(Console c, ushort addr) {
        c.SetMemory(0x2006, (byte)(addr >> 8));
        c.SetMemory(0x2006, (byte)(addr & 0xFF));
    }

    private static byte ReadPpuDataAfterSeek(Console c, ushort addr) {
        SetPpuAddr(c, addr);
        _ = c.GetMemory(0x2007);
        return c.GetMemory(0x2007);
    }

    private static void WritePpuData(Console c, ushort addr, byte value) {
        SetPpuAddr(c, addr);
        c.SetMemory(0x2007, value);
    }

    [Fact]
    public void PPUCTRL0x2000WriteThenReadReturnsLastValue()
    {
        var console = new Console();
        console.SetMemory(0x2000, 0x85);
        Assert.Equal(0x85, console.GetMemory(0x2000));
    }

    [Fact]
    public void PPUCTRLMirroredAt0x20080x20100x2018()
    {
        var console = new Console();
        console.SetMemory(0x2008, 0xAA);
        Assert.Equal(0xAA, console.GetMemory(0x2000));
        Assert.Equal(0xAA, console.GetMemory(0x2008));
    }

    [Fact]
    public void PPUMASK0x2001WriteThenReadReturnsLastValue()
    {
        var console = new Console();
        console.SetMemory(0x2001, 0x1E);
        Assert.Equal(0x1E, console.GetMemory(0x2001));
    }

    [Fact]
    public void ReadPPUSTATUS0x2002ClearsVBlankAndScrollAddrLatch()
    {
        var console = new Console();
        console.SetMemory(0x2000, 0x80);
        _ = console.GetMemory(0x2002);
        console.SetMemory(0x2005, 0x11);
        console.SetMemory(0x2005, 0x22);
        Assert.Equal(0x11, console.GetMemory(0x2005));
    }

    [Fact]
    public void OAMADDR0x2003WriteThenReadReturnsLastValue()
    {
        var console = new Console();
        console.SetMemory(0x2003, 0x42);
        Assert.Equal(0x42, console.GetMemory(0x2003));
    }

    [Fact]
    public void PPUSCROLL0x2005FirstWriteXSecondWriteY()
    {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        console.SetMemory(0x2005, 0x11);
        console.SetMemory(0x2005, 0x22);
        Assert.Equal(0x11, console.GetMemory(0x2005));
    }

    [Fact]
    public void PPUADDR0x2006TwoWritesHighThenLow()
    {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        console.SetMemory(0x2006, 0x21);
        console.SetMemory(0x2006, 0x08);
        console.SetMemory(0x2007, 0x00);
        var hi = console.GetMemory(0x2006);
        Assert.Equal(0x21, hi);
    }

    [Fact]
    public void PPUDATA0x2007FirstReadAfterWriteReturnsWrittenValueFromBuffer()
    {
        var console = new Console();
        console.SetMemory(0x2000, 0x00);
        console.SetMemory(0x2006, 0x20);
        console.SetMemory(0x2006, 0x00);
        console.SetMemory(0x2007, 0xAB);
        var firstRead = console.GetMemory(0x2007);
        Assert.Equal(0xAB, firstRead);
    }

    [Theory]
    [InlineData(0x2000, 0x2000)]
    [InlineData(0x2000, 0x2008)]
    [InlineData(0x2000, 0x2010)]
    [InlineData(0x2001, 0x2001)]
    [InlineData(0x2001, 0x2009)]
    [InlineData(0x2003, 0x2003)]
    [InlineData(0x2005, 0x2005)]
    [InlineData(0x2006, 0x2006)]
    public void PPURegisterMirroredEvery8Bytes(ushort writeAddr, ushort readAddr)
    {
        var console = new Console();
        var val = (byte)((writeAddr & 0x0F) | 0xA0);
        console.SetMemory(writeAddr, val);
        Assert.Equal(val, console.GetMemory(readAddr));
    }

    [Fact]
    public void Read_PPUSTATUS_resets_PPUSCROLL_latch_to_first_write()
    {
        var console = new Console();
        console.SetMemory(0x2005, 0x11);
        console.SetMemory(0x2005, 0x22);
        _ = console.GetMemory(0x2002);
        console.SetMemory(0x2005, 0x33);
        Assert.Equal(0x33, console.GetMemory(0x2005));
    }

    [Fact]
    public void PPUADDRIncrementsBy1WhenPPUCTRLBit2Clear()
    {
        var console = new Console();
        console.SetMemory(0x2000, 0x00);
        SetPpuAddr(console, 0x2000);
        console.SetMemory(0x2007, 0xAA);
        console.SetMemory(0x2007, 0xBB);
        SetPpuAddr(console, 0x2000);
        _ = console.GetMemory(0x2007);
        Assert.Equal(0xAA, console.GetMemory(0x2007));
        Assert.Equal(0xBB, console.GetMemory(0x2007));
    }

    [Fact]
    public void PPUADDRIncrementsBy32WhenPPUCTRLBit2Set()
    {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        console.SetMemory(0x2000, 0x04);
        SetPpuAddr(console, 0x2000);
        console.SetMemory(0x2007, 0xBB);
        console.SetMemory(0x2007, 0xCC);
        SetPpuAddr(console, 0x2000);
        _ = console.GetMemory(0x2007);
        Assert.Equal(0xBB, console.GetMemory(0x2007));
        SetPpuAddr(console, 0x2020);
        _ = console.GetMemory(0x2007);
        Assert.Equal(0xCC, console.GetMemory(0x2007));
    }

    // Name table mirroring per Specification/Other/nestech.txt (G. Name Table Mirroring) and nes.txt:
    //   Horizontal: NT#0 $000, NT#1 $000, NT#2 $400, NT#3 $400  => ($2000,$2400) same, ($2800,$2C00) same
    //   Vertical:   NT#0 $000, NT#1 $400, NT#2 $000, NT#3 $400  => ($2000,$2800) same, ($2400,$2C00) same
    // nes.txt: "With vertical mirroring, tables 2 and 3 are the mirrors of pages 0 and 1. With horizontal mirroring, pages 1 and 3 are the mirrors of pages 0 and 2."

    [Fact]
    public void NametableMirroringMatchesNestechAndNesSpec()
    {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        // Vertical: $2000↔$2800, $2400↔$2C00 (nestech.txt table)
        console.SetPpuMirroring(Mirroring.Vertical);
        WritePpuData(console, 0x2000, 0xA0);
        WritePpuData(console, 0x2800, 0xB0);
        Assert.Equal(0xB0, ReadPpuDataAfterSeek(console, 0x2000));
        WritePpuData(console, 0x2400, 0xC0);
        WritePpuData(console, 0x2C00, 0xD0);
        Assert.Equal(0xD0, ReadPpuDataAfterSeek(console, 0x2400));
        // Horizontal: $2000↔$2400, $2800↔$2C00 (nestech.txt table)
        console.SetPpuMirroring(Mirroring.Horizontal);
        WritePpuData(console, 0x2000, 0xE0);
        WritePpuData(console, 0x2400, 0xF0);
        Assert.Equal(0xF0, ReadPpuDataAfterSeek(console, 0x2000));
        WritePpuData(console, 0x2800, 0x11);
        WritePpuData(console, 0x2C00, 0x22);
        Assert.Equal(0x22, ReadPpuDataAfterSeek(console, 0x2800));
    }

    [Fact]
    public void VerticalMirroring2000And2800SamePhysical()
    {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        console.SetPpuMirroring(Mirroring.Vertical);
        SetPpuAddr(console, 0x2000);
        WritePpuData(console, 0x2000, 0x11);
        WritePpuData(console, 0x2800, 0x22);
        var at2000 = ReadPpuDataAfterSeek(console, 0x2000);
        Assert.Equal(0x22, at2000);
    }

    [Fact]
    public void VerticalMirroring2400And2C00SamePhysical()
    {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        console.SetPpuMirroring(Mirroring.Vertical);
        SetPpuAddr(console, 0x2400);
        WritePpuData(console, 0x2400, 0x33);
        WritePpuData(console, 0x2C00, 0x44);
        var at2400 = ReadPpuDataAfterSeek(console, 0x2400);
        Assert.Equal(0x44, at2400);
    }

    [Fact]
    public void HorizontalMirroring2000And2400SamePhysical()
    {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        console.SetPpuMirroring(Mirroring.Horizontal);
        SetPpuAddr(console, 0x2000);
        WritePpuData(console, 0x2000, 0x55);
        WritePpuData(console, 0x2400, 0x66);
        var at2000 = ReadPpuDataAfterSeek(console, 0x2000);
        Assert.Equal(0x66, at2000);
    }

    [Fact]
    public void HorizontalMirroring2800And2C00SamePhysical()
    {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        console.SetPpuMirroring(Mirroring.Horizontal);
        SetPpuAddr(console, 0x2800);
        WritePpuData(console, 0x2800, 0x77);
        WritePpuData(console, 0x2C00, 0x88);
        var at2800 = ReadPpuDataAfterSeek(console, 0x2800);
        Assert.Equal(0x88, at2800);
    }

}
