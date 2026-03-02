using System;
using System.IO;
using System.Linq;
using ASD.NES.Core;
using Xunit;

namespace ASD.NES.Tests;

/// <summary>
/// iNES header tests per "iNES Header Format Information File" and NESDEV.
/// Bytes 0-3: "NES\x1A". Byte 4: PRG pages (16 KiB each). Byte 5: CHR pages (8 KiB each).
/// Byte 6: lower nibble = mirroring/trainer/battery/4-screen; high nibble = mapper low.
/// Byte 7: high nibble = mapper high (mapper = (byte7 & 0xF0) | (byte6 >> 4)).
/// Bytes 8-15: zero for standard; flags 9 (bit 0) and 10 (bits 0-1) for TV system.
/// </summary>
public sealed class INesHeaderTests
{
    private static byte[] BuildRom(
        int prgPages = 1,
        int chrPages = 1,
        byte byte6 = 0,
        byte byte7 = 0,
        byte[]? bytes8to15 = null,
        bool withTrainer = false)
    {
        var headerLen = 16;
        var trainerLen = withTrainer ? 512 : 0;
        var prgLen = prgPages * 0x4000;
        var chrLen = chrPages * 0x2000;
        var data = new byte[headerLen + trainerLen + prgLen + chrLen];

        data[0] = 0x4E;
        data[1] = 0x45;
        data[2] = 0x53;
        data[3] = 0x1A;
        data[4] = (byte)prgPages;
        data[5] = (byte)chrPages;
        data[6] = byte6;
        data[7] = byte7;
        for (var i = 8; i < 16; i++) {
            data[i] = bytes8to15 != null && i - 8 < bytes8to15.Length ? bytes8to15[i - 8] : (byte)0;
        }

        for (var i = headerLen + trainerLen; i < data.Length; i++) {
            data[i] = 0xFF;
        }
        return data;
    }

    [Fact]
    public void Valid_signature_creates_cartridge()
    {
        var rom = BuildRom(1, 1);
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
    }

    [Fact]
    public void Invalid_signature_throws()
    {
        var rom = BuildRom(1, 1);
        rom[0] = 0x00;
        Assert.Throws<InvalidDataException>(() => Cartridge.Create(rom));
    }

    [Fact]
    public void Signature_must_be_NES_0x1A()
    {
        var rom = BuildRom(1, 1);
        rom[3] = 0x1B;
        Assert.Throws<InvalidDataException>(() => Cartridge.Create(rom));
    }

    [Fact]
    public void Region_NTSC_when_flags_9_and_10_not_PAL()
    {
        var rom = BuildRom(1, 1, bytes8to15: new byte[8]);
        var cart = Cartridge.Create(rom);
        Assert.Equal(TvRegion.NTSC, cart.Region);
    }

    [Fact]
    public void Region_PAL_when_flag_9_bit0_set()
    {
        var rom = BuildRom(1, 1, bytes8to15: new byte[] { 0, 1, 0, 0, 0, 0, 0, 0 });
        var cart = Cartridge.Create(rom);
        Assert.Equal(TvRegion.PAL, cart.Region);
    }

    [Fact]
    public void Region_PAL_when_flag_10_bits_0_1_equal_2()
    {
        var rom = BuildRom(1, 1, bytes8to15: new byte[] { 0, 0, 2, 0, 0, 0, 0, 0 });
        var cart = Cartridge.Create(rom);
        Assert.Equal(TvRegion.PAL, cart.Region);
    }

    [Fact]
    public void One_PRG_one_CHR_minimal_size()
    {
        var rom = BuildRom(1, 1);
        Assert.Equal(16 + 0x4000 + 0x2000, rom.Length);
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
    }

    [Fact]
    public void Trainer_offset_16_plus_512()
    {
        var rom = BuildRom(1, 1, byte6: 0x04, withTrainer: true); // bit 2 = trainer
        Assert.True(rom.Length >= 16 + 512 + 0x4000 + 0x2000);
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
    }

    [Fact]
    public void Two_PRG_pages_acceptable()
    {
        var rom = BuildRom(2, 1);
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
    }

    [Fact]
    public void Zero_CHR_pages_embedded_CHR_supported()
    {
        var rom = BuildRom(1, 0);
        Assert.Equal(16 + 0x4000, rom.Length);
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
    }
}
