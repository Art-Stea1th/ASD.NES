using System;
using ASD.NES.Core;
using Xunit;
using Console = ASD.NES.Core.Console;

namespace ASD.NES.Tests;

/// <summary>
/// CPU memory map per NESMemory.txt and nestech.txt: WRAM $0000-$07FF and mirrors at $0800-$1FFF.
/// </summary>
[Collection("CPU")]
public sealed class MemoryTests
{
    [Fact]
    public void WRAMZeroPageWritableAndReadable()
    {
        var console = new Console();
        console.SetMemory(0x00, 0xAB);
        console.SetMemory(0xFF, 0xCD);
        Assert.Equal(0xAB, console.GetMemory(0x00));
        Assert.Equal(0xCD, console.GetMemory(0xFF));
    }

    [Fact]
    public void WRAM0200To07FFWritableAndReadable()
    {
        var console = new Console();
        console.SetMemory(0x0200, 0x11);
        console.SetMemory(0x07FF, 0x22);
        Assert.Equal(0x11, console.GetMemory(0x0200));
        Assert.Equal(0x22, console.GetMemory(0x07FF));
    }

    [Fact]
    public void WRAMMirroredAt0800To0FFF()
    {
        var console = new Console();
        console.SetMemory(0x0000, 0xA1);
        console.SetMemory(0x0100, 0xB2);
        console.SetMemory(0x07FF, 0xC3);
        Assert.Equal(0xA1, console.GetMemory(0x0800));
        Assert.Equal(0xB2, console.GetMemory(0x0900));
        Assert.Equal(0xC3, console.GetMemory(0x0FFF));
    }

    [Fact]
    public void WRAMMirroredAt1000And1800()
    {
        var console = new Console();
        console.SetMemory(0x0000, 0x77);
        Assert.Equal(0x77, console.GetMemory(0x0800));
        Assert.Equal(0x77, console.GetMemory(0x1000));
        Assert.Equal(0x77, console.GetMemory(0x1800));
    }

    [Fact]
    public void WriteToMirrorUpdatesSamePhysicalLocation()
    {
        var console = new Console();
        console.SetMemory(0x0800, 0x99);
        Assert.Equal(0x99, console.GetMemory(0x0000));
        console.SetMemory(0x1FFF, 0x66);
        Assert.Equal(0x66, console.GetMemory(0x07FF));
    }
}
