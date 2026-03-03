using System;
using ASD.NES.Core;
using ASD.NES.Core.ConsoleComponents.PPUParts;
using Xunit;
using Console = ASD.NES.Core.Console;

namespace ASD.NES.Tests;

/// <summary>
/// Tests for PPU background scroll formula (NESDEV 512x480 virtual space, nametable index, map coords).
/// Nametable mirroring expectations from Specification/Other/nestech.txt (G. Name Table Mirroring)
/// and Specification/Other/nes.txt: Vertical => NT0+NT2 same, NT1+NT3 same; Horizontal => NT0+NT1 same, NT2+NT3 same.
/// </summary>
[Collection("CPU")]
public sealed class PpuScrollFormulaTests
{
    private static void Coords(int startX, int startY, int scrollX, int scrollY, int scanpoint, int scanline,
        out int nt, out int mapX, out int mapY) {
        ScrollFormula.GetBackgroundCoords(startX, startY, scrollX, scrollY, scanpoint, scanline, out nt, out mapX, out mapY);
    }

    [Fact]
    public void Scroll00Scanline0Scanpoint0GivesNametable0Map00() {
        Coords(0, 0, 0, 0, 0, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(0, nt);
        Assert.Equal(0, mapX);
        Assert.Equal(0, mapY);
    }

    [Fact]
    public void Scroll2560GivesNametable1Map00() {
        Coords(0, 0, 256, 0, 0, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(1, nt);
        Assert.Equal(0, mapX);
        Assert.Equal(0, mapY);
    }

    [Fact]
    public void Scroll0240GivesNametable2Map00() {
        Coords(0, 0, 0, 240, 0, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(2, nt);
        Assert.Equal(0, mapX);
        Assert.Equal(0, mapY);
    }

    [Fact]
    public void Scroll2600Scanpoint0GivesNametable1Map40() {
        Coords(0, 0, 260, 0, 0, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(1, nt);
        Assert.Equal(4, mapX);
        Assert.Equal(0, mapY);
    }

    [Fact]
    public void Scroll0250Scanline0GivesNametable2Map010() {
        Coords(0, 0, 0, 250, 0, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(2, nt);
        Assert.Equal(0, mapX);
        Assert.Equal(10, mapY);
    }

    [Fact]
    public void StartY240Scroll0Scanline0GivesNametable2() {
        Coords(0, 240, 0, 0, 0, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(2, nt);
        Assert.Equal(0, mapX);
        Assert.Equal(0, mapY);
    }

    [Fact]
    public void WrapY250Plus239ScanlineWrapsToTopNametable() {
        Coords(0, 0, 0, 250, 0, 239, out int nt, out int mapX, out int mapY);
        Assert.Equal(0, nt);
        Assert.Equal(0, mapX);
        Assert.Equal(9, mapY);
    }

    [Fact]
    public void WrapX300Plus256ScanpointWrapsToLeftNametable() {
        Coords(0, 0, 300, 0, 256, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(0, nt);
        Assert.Equal(44, mapX);
        Assert.Equal(0, mapY);
    }

    /// <summary>Per nestech.txt: Horizontal has NT#0 $000, NT#1 $000 — so $2000 and $2400 are same physical; scroll 0 vs 256 reads different logical NTs, different tiles.</summary>
    [Fact]
    public void HorizontalMirroringScroll0ReadsTileFrom2000Scroll256From2400() {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        console.SetPpuMirroring(Mirroring.Horizontal);
        console.SetMemory(0x2006, 0x20);
        console.SetMemory(0x2006, 0x00);
        console.SetMemory(0x2007, 0x11);
        console.SetMemory(0x2006, 0x24);
        console.SetMemory(0x2006, 0x00);
        console.SetMemory(0x2007, 0x22);
        ScrollFormula.GetBackgroundCoords(0, 0, 0, 0, 0, 0, out int nt0, out int mx0, out int my0);
        ScrollFormula.GetBackgroundCoords(0, 0, 256, 0, 0, 0, out int nt1, out int mx1, out int my1);
        var tile0 = PPUAddressSpace.Instance.GetNametable(nt0).GetSymbol(mx0 >> 3, my0 >> 3);
        var tile1 = PPUAddressSpace.Instance.GetNametable(nt1).GetSymbol(mx1 >> 3, my1 >> 3);
        Assert.Equal(0, nt0);
        Assert.Equal(0x11, tile0);
        Assert.Equal(1, nt1);
        Assert.Equal(0x22, tile1);
    }

    /// <summary>Per nestech.txt: Vertical has NT#0 $000, NT#2 $000 — $2000 and $2800 same physical; scroll 0 and 240 both read that bank at (0,0), same tile.</summary>
    [Fact]
    public void VerticalMirroringScroll0ReadsTileFromTopScroll240FromBottom() {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        console.SetPpuMirroring(Mirroring.Vertical);
        // Vertical: $2000 (NT0) and $2800 (NT2) are the same physical bank (nestech.txt table).
        console.SetMemory(0x2006, 0x20);
        console.SetMemory(0x2006, 0x00);
        console.SetMemory(0x2007, 0x11);
        ScrollFormula.GetBackgroundCoords(0, 0, 0, 0, 0, 0, out int nt0, out int mx0, out int my0);
        ScrollFormula.GetBackgroundCoords(0, 0, 0, 240, 0, 0, out int nt2, out int mx2, out int my2);
        var tileTop = PPUAddressSpace.Instance.GetNametable(nt0).GetSymbol(mx0 >> 3, my0 >> 3);
        var tileBot = PPUAddressSpace.Instance.GetNametable(nt2).GetSymbol(mx2 >> 3, my2 >> 3);
        Assert.Equal(0, nt0);
        Assert.Equal(2, nt2);
        Assert.Equal(0x11, tileTop);
        Assert.Equal(0x11, tileBot); // NT2 mirrors NT0 in Vertical, so same tile
    }

    [Fact]
    public void OneFrameRendersWithScroll() {
        var rom = new byte[16 + 16384 + 8192];
        rom[0] = 0x4E;
        rom[1] = 0x45;
        rom[2] = 0x53;
        rom[3] = 0x1A;
        rom[4] = 1;
        rom[5] = 1;
        var offset = 16;
        rom[offset + 0] = 0x4C;
        rom[offset + 1] = 0x00;
        rom[offset + 2] = 0x80;
        rom[offset + 0x3FFC] = 0x00;
        rom[offset + 0x3FFD] = 0x80;
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        _ = console.GetMemory(0x2002);
        console.SetMemory(0x2005, 10);
        console.SetMemory(0x2005, 20);
        console.SetMemory(0x2000, 0);
        var frame = console.Update();
        Assert.NotNull(frame);
        Assert.Equal(256 * 240, frame.Length);
    }
}
