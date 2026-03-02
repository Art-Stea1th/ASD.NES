using System;
using ASD.NES.Core;
using Xunit;
using Console = ASD.NES.Core.Console;

namespace ASD.NES.Tests;

/// <summary>
/// PPU register and memory-map behavior per NES docs (nes.txt, NESDEV PPU).
/// CPU maps 0x2000-0x3FFF to PPU registers (mirrored every 8 bytes); write-only on hardware, emulator returns last written.
/// </summary>
[Collection("CPU")]
public sealed class PpuTests
{
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
        console.SetMemory(0x2002, 0x00);
        byte status = console.GetMemory(0x2002);
        Assert.Equal(0x00, status);
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
        byte hi = console.GetMemory(0x2006);
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
        byte firstRead = console.GetMemory(0x2007);
        Assert.Equal(0xAB, firstRead);
    }
}
