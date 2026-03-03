using System;
using ASD.NES.Core;
using Xunit;
using Console = ASD.NES.Core.Console;

namespace ASD.NES.Tests;

/// <summary>
/// Input ports $4016 (P1) and $4017 (P2) per nes.txt/nestech.txt: strobe (write $4016 bit0 1 then 0) reloads shift registers;
/// each read returns next button bit (0 or 1); after 8 reads further reads return 0.
/// </summary>
[Collection("CPU")]
public sealed class InputTests
{
    [Fact]
    public void StrobeThenRead4016ReturnsEightButtonBitsThenZeros()
    {
        var console = new Console();
        var pad = new MockGamepad { State = 0xFF };
        console.PlayerOneController = pad;
        console.SetMemory(0x4016, 1);
        console.SetMemory(0x4016, 0);
        for (var i = 0; i < 8; i++) {
            Assert.Equal(1, console.GetMemory(0x4016) & 1);
        }
        for (var i = 0; i < 8; i++) {
            Assert.Equal(0, console.GetMemory(0x4016) & 1);
        }
    }

    [Fact]
    public void StrobeThenRead4016ReturnsOnlyAPressed()
    {
        var console = new Console();
        var pad = new MockGamepad { State = 0x01 };
        console.PlayerOneController = pad;
        console.SetMemory(0x4016, 1);
        console.SetMemory(0x4016, 0);
        Assert.Equal(1, console.GetMemory(0x4016) & 1);
        for (var i = 1; i < 8; i++) {
            Assert.Equal(0, console.GetMemory(0x4016) & 1);
        }
    }

    [Fact]
    public void BothControllersStrobeThenRead4016And4017ReturnBits()
    {
        var console = new Console();
        var pad1 = new MockGamepad { State = 0x01 };
        var pad2 = new MockGamepad { State = 0x01 };
        console.PlayerOneController = pad1;
        console.PlayerTwoController = pad2;
        console.SetMemory(0x4016, 1);
        console.SetMemory(0x4016, 0);
        Assert.Equal(1, console.GetMemory(0x4016) & 1);
        Assert.Equal(1, console.GetMemory(0x4017) & 1);
    }

    [Fact]
    public void NoKeysPressedRead4016ReturnsZero()
    {
        var console = new Console();
        var pad = new MockGamepad { State = 0 };
        console.PlayerOneController = pad;
        console.SetMemory(0x4016, 1);
        console.SetMemory(0x4016, 0);
        for (var i = 0; i < 8; i++) {
            Assert.Equal(0, console.GetMemory(0x4016) & 1);
        }
    }
}
