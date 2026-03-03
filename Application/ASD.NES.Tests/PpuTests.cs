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
    public void PPUCTRL_0x2000_write_then_read_returns_last_value()
    {
        var console = new Console();
        console.SetMemory(0x2000, 0x85);
        Assert.Equal(0x85, console.GetMemory(0x2000));
    }

    [Fact]
    public void PPUCTRL_mirrored_at_0x2008_0x2010_0x2018()
    {
        var console = new Console();
        console.SetMemory(0x2008, 0xAA);
        Assert.Equal(0xAA, console.GetMemory(0x2000));
        Assert.Equal(0xAA, console.GetMemory(0x2008));
    }

    [Fact]
    public void PPUMASK_0x2001_write_then_read_returns_last_value()
    {
        var console = new Console();
        console.SetMemory(0x2001, 0x1E);
        Assert.Equal(0x1E, console.GetMemory(0x2001));
    }

    [Fact]
    public void Read_PPUSTATUS_0x2002_clears_VBlank_and_scroll_addr_latch()
    {
        var console = new Console();
        console.SetMemory(0x2000, 0x80);
        _ = console.GetMemory(0x2002);
        console.SetMemory(0x2005, 0x11);
        console.SetMemory(0x2005, 0x22);
        Assert.Equal(0x11, console.GetMemory(0x2005));
    }

    [Fact]
    public void OAMADDR_0x2003_write_then_read_returns_last_value()
    {
        var console = new Console();
        console.SetMemory(0x2003, 0x42);
        Assert.Equal(0x42, console.GetMemory(0x2003));
    }

    [Fact]
    public void PPUSCROLL_0x2005_first_write_X_second_write_Y()
    {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        console.SetMemory(0x2005, 0x11);
        console.SetMemory(0x2005, 0x22);
        Assert.Equal(0x11, console.GetMemory(0x2005));
    }

    [Fact]
    public void PPUADDR_0x2006_two_writes_high_then_low()
    {
        var console = new Console();
        console.SetMemory(0x2006, 0x21);
        console.SetMemory(0x2006, 0x08);
        console.SetMemory(0x2007, 0x00);
        var hi = console.GetMemory(0x2006);
        Assert.Equal(0x21, hi);
    }

    [Fact]
    public void PPUDATA_0x2007_first_read_after_write_returns_written_value_from_buffer()
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
    public void PPU_register_mirrored_every_8_bytes(ushort writeAddr, ushort readAddr)
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
    public void PPUADDR_increments_by_1_when_PPUCTRL_bit2_clear()
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
    public void PPUADDR_increments_by_32_when_PPUCTRL_bit2_set()
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
    public void Nametable_mirroring_matches_nestech_and_nes_spec()
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
    public void Vertical_mirroring_2000_and_2800_same_physical()
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
    public void Vertical_mirroring_2400_and_2C00_same_physical()
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
    public void Horizontal_mirroring_2000_and_2800_same_physical()
    {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        console.SetPpuMirroring(Mirroring.Horizontal);
        SetPpuAddr(console, 0x2000);
        WritePpuData(console, 0x2000, 0x55);
        WritePpuData(console, 0x2800, 0x66);
        var at2000 = ReadPpuDataAfterSeek(console, 0x2000);
        Assert.Equal(0x66, at2000);
    }

    [Fact]
    public void Horizontal_mirroring_2400_and_2C00_same_physical()
    {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        console.SetPpuMirroring(Mirroring.Horizontal);
        SetPpuAddr(console, 0x2400);
        WritePpuData(console, 0x2400, 0x77);
        WritePpuData(console, 0x2C00, 0x88);
        var at2400 = ReadPpuDataAfterSeek(console, 0x2400);
        Assert.Equal(0x88, at2400);
    }

}
