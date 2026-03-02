using System;
using ASD.NES.Core;
using ASD.NES.Core.ConsoleComponents.PPUParts;
using Xunit;
using Console = ASD.NES.Core.Console;

namespace ASD.NES.Tests;

/// <summary>
/// Tests for PPU background scroll formula (NESDEV 512x480 virtual space, nametable index, map coords).
/// </summary>
[Collection("CPU")]
public sealed class PpuScrollFormulaTests
{
    private static void Coords(int startX, int startY, int scrollX, int scrollY, int scanpoint, int scanline,
        out int nt, out int mapX, out int mapY) {
        ScrollFormula.GetBackgroundCoords(startX, startY, scrollX, scrollY, scanpoint, scanline, out nt, out mapX, out mapY);
    }

    [Fact]
    public void Scroll_0_0_scanline_0_scanpoint_0_gives_nametable_0_map_0_0() {
        Coords(0, 0, 0, 0, 0, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(0, nt);
        Assert.Equal(0, mapX);
        Assert.Equal(0, mapY);
    }

    [Fact]
    public void Scroll_256_0_gives_nametable_1_map_0_0() {
        Coords(0, 0, 256, 0, 0, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(1, nt);
        Assert.Equal(0, mapX);
        Assert.Equal(0, mapY);
    }

    [Fact]
    public void Scroll_0_240_gives_nametable_2_map_0_0() {
        Coords(0, 0, 0, 240, 0, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(2, nt);
        Assert.Equal(0, mapX);
        Assert.Equal(0, mapY);
    }

    [Fact]
    public void Scroll_260_0_scanpoint_0_gives_nametable_1_map_4_0() {
        Coords(0, 0, 260, 0, 0, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(1, nt);
        Assert.Equal(4, mapX);
        Assert.Equal(0, mapY);
    }

    [Fact]
    public void Scroll_0_250_scanline_0_gives_nametable_2_map_0_10() {
        Coords(0, 0, 0, 250, 0, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(2, nt);
        Assert.Equal(0, mapX);
        Assert.Equal(10, mapY);
    }

    [Fact]
    public void StartY_240_scroll_0_scanline_0_gives_nametable_2() {
        Coords(0, 240, 0, 0, 0, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(2, nt);
        Assert.Equal(0, mapX);
        Assert.Equal(0, mapY);
    }

    [Fact]
    public void Wrap_Y_250_plus_239_scanline_wraps_to_top_nametable() {
        Coords(0, 0, 0, 250, 0, 239, out int nt, out int mapX, out int mapY);
        Assert.Equal(0, nt);
        Assert.Equal(0, mapX);
        Assert.Equal(9, mapY);
    }

    [Fact]
    public void Wrap_X_300_plus_256_scanpoint_wraps_to_left_nametable() {
        Coords(0, 0, 300, 0, 256, 0, out int nt, out int mapX, out int mapY);
        Assert.Equal(0, nt);
        Assert.Equal(44, mapX);
        Assert.Equal(0, mapY);
    }

    [Fact]
    public void Horizontal_mirroring_scroll_0_reads_tile_from_2000_scroll_256_from_2400() {
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

    [Fact]
    public void Vertical_mirroring_scroll_0_reads_tile_from_top_scroll_240_from_bottom() {
        var console = new Console();
        _ = console.GetMemory(0x2002);
        console.SetPpuMirroring(Mirroring.Vertical);
        console.SetMemory(0x2006, 0x20);
        console.SetMemory(0x2006, 0x00);
        console.SetMemory(0x2007, 0x11);
        console.SetMemory(0x2006, 0x28);
        console.SetMemory(0x2006, 0x00);
        console.SetMemory(0x2007, 0x33);
        ScrollFormula.GetBackgroundCoords(0, 0, 0, 0, 0, 0, out int nt0, out int mx0, out int my0);
        ScrollFormula.GetBackgroundCoords(0, 0, 0, 240, 0, 0, out int nt2, out int mx2, out int my2);
        var tileTop = PPUAddressSpace.Instance.GetNametable(nt0).GetSymbol(mx0 >> 3, my0 >> 3);
        var tileBot = PPUAddressSpace.Instance.GetNametable(nt2).GetSymbol(mx2 >> 3, my2 >> 3);
        Assert.Equal(0, nt0);
        Assert.Equal(0x11, tileTop);
        Assert.Equal(2, nt2);
        Assert.Equal(0x33, tileBot);
    }

    [Fact]
    public void One_frame_renders_with_scroll() {
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
