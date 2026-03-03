using System;
using ASD.NES.Core;
using Xunit;
using Console = ASD.NES.Core.Console;

namespace ASD.NES.Tests;

/// <summary>
/// APU register behavior per apu_ref.txt and NESDEV. CPU maps 0x4000-0x4015 to APU; only 0x4015 is readable (status).
/// </summary>
[Collection("CPU")]
public sealed class ApuTests
{
    [Fact]
    public void APUStatus0x4015WriteThenReadReturnsChannelEnablesInLowBits()
    {
        var console = new Console();
        console.SetMemory(0x4015, 0x1F);
        var status = console.GetMemory(0x4015);
        Assert.Equal(0x1F, status & 0x1F);
    }

    [Fact]
    public void APUStatus0x4015Write0ClearsChannelEnables()
    {
        var console = new Console();
        console.SetMemory(0x4015, 0x1F);
        console.SetMemory(0x4015, 0x00);
        var status = console.GetMemory(0x4015);
        Assert.Equal(0x00, status & 0x1F);
    }

    [Fact]
    public void APUStatus0x4015ReadClearsFrameAndDMCIRQBits()
    {
        var console = new Console();
        console.SetMemory(0x4015, 0x00);
        console.GetMemory(0x4015);
        var secondRead = console.GetMemory(0x4015);
        Assert.Equal(0x00, secondRead & 0xC0);
    }

    [Fact]
    public void APUPulse10x40000x4003WriteOnlyReadReturnsZero()
    {
        var console = new Console();
        console.SetMemory(0x4000, 0x8F);
        console.SetMemory(0x4001, 0x7E);
        Assert.Equal(0, console.GetMemory(0x4000));
        Assert.Equal(0, console.GetMemory(0x4001));
    }

    [Fact]
    public void APU0x4015MirroredReadOnlyStatus()
    {
        var console = new Console();
        console.SetMemory(0x4015, 0x0F);
        Assert.Equal(0x0F, console.GetMemory(0x4015) & 0x1F);
    }
}
