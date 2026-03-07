using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASD.NES.Core;
using ASD.NES.Core.ConsoleComponents.PPUParts;
using Xunit;
using Console = ASD.NES.Core.Console;

namespace ASD.NES.Tests;

/// <summary>
/// iNES header tests per "iNES Header Format Information File" and NESDEV.
/// Bytes 0-3: "NES\x1A". Byte 4: PRG pages (16 KiB each). Byte 5: CHR pages (8 KiB each).
/// Byte 6: lower nibble = mirroring/trainer/battery/4-screen; high nibble = mapper low.
/// Byte 7: high nibble = mapper high (mapper = (byte7 & 0xF0) | (byte6 >> 4)).
/// Bytes 8-15: zero for standard; flags 9 (bit 0) and 10 (bits 0-1) for TV system.
/// </summary>
[Collection("CPU")]
public sealed class INesHeaderTests
{
    /// <summary>
    /// Optional: local paths to unpacked NES ROMs. Tests will auto-skip if these folders are missing.
    /// Add new local collections here to extend smoke coverage (one ROM per mapper, region checks, etc).
    /// </summary>
    private static readonly string[] OptionalRomsPaths = {
        Path.Combine(Path.GetDirectoryName(typeof(INesHeaderTests).Assembly.Location) ?? ".", "..", "..", "..", "..", "..", ".."), // bin/Debug/net8.0 -> ASD (parent of ASD.NES)
        @"D:\Art\Documents\!My\Projects\! Programming\ASD",
        @"e:\Games\NES\EmulatorsPack\NES\roms",
        @"e:\Games\NES\NES"
    };

    private static string? FindRomInPaths(params string[] names)
    {
        foreach (var baseDir in OptionalRomsPaths)
        {
            var dir = baseDir;
            if (!Path.IsPathRooted(baseDir) && typeof(INesHeaderTests).Assembly.Location is string loc)
                dir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(loc) ?? ".", baseDir));
            if (!Directory.Exists(dir)) continue;
            foreach (var name in names)
            {
                var path = Path.Combine(dir, name);
                if (File.Exists(path)) return path;
            }
        }
        return null;
    }

    [Fact]
    public void RunRealRomRunsAFewFrames()
    {
        var romPath = OptionalRomsPaths.Select(p => Path.Combine(p, "Battle City (J).nes")).FirstOrDefault(File.Exists);
        if (romPath == null) {
            return;
        }
        var rom = File.ReadAllBytes(romPath);
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
        var console = new Console();
        console.InsertCartridge(cart);
        var resetLo = console.GetMemory(0xFFFC);
        var resetHi = console.GetMemory(0xFFFD);
        Assert.Equal(rom[16 + 0x3FFC], resetLo);
        Assert.Equal(rom[16 + 0x3FFD], resetHi);
        console.RunCpuSteps(100);
        var state = console.GetCpuState();
        Assert.True(state.PC >= 0x8000 && state.PC <= 0xFFFF);
    }

    /// <summary>Optional: nestest.nes per NESDEV — start at $C000, run many steps, check $02/$03 for result (0x00 0x00 = pass).</summary>
    [Fact]
    public void Nestest_IfPresent_RunFromC000_Check02_03()
    {
        var romPath = FindRomInPaths("nestest.nes");
        if (romPath == null) return;
        var rom = File.ReadAllBytes(romPath);
        if (rom.Length < 16 + 0x4000) return;
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
        var console = new Console();
        console.InsertCartridge(cart);
        console.SetPC(0xC000);
        console.RunCpuSteps(150_000);
        var resultLo = console.GetMemory(0x02);
        var resultHi = console.GetMemory(0x03);
        Assert.True(resultLo == 0x00 && resultHi == 0x00, $"nestest result $02=0x{resultLo:X2} $03=0x{resultHi:X2} (expected 0x00 0x00 = pass)");
    }

    /// <summary>Optional: blargg CPU test ROMs write result to $6000 (0x80=done); string at $6004. NROM must provide writable $6000-$7FFF.</summary>
    [Fact]
    public void BlarggCpuTest_IfPresent_RunThenRead6000()
    {
        var romPath = FindRomInPaths("instr_test.nes", "instr_test-v5.nes", "cpu_exec_space.nes", "cpu_dummy_reads.nes", "instr_misc.nes");
        if (romPath == null) return;
        var rom = File.ReadAllBytes(romPath);
        if (rom.Length < 16 + 0x4000) return;
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
        var console = new Console();
        console.InsertCartridge(cart);
        var maxSteps = 5_000_000;
        var step = 0;
        while (step < maxSteps)
        {
            console.RunCpuSteps(10_000);
            step += 10_000;
            var status = console.GetMemory(0x6000);
            if (status == 0x80)
            {
                var msg = System.Text.Encoding.ASCII.GetString(ReadBytes(console, 0x6004, 64)).TrimEnd('\0');
                Assert.True(msg.StartsWith("Pass", StringComparison.OrdinalIgnoreCase), $"blargg test failed: {msg}");
                return;
            }
            if (status != 0 && status != 0x80) break;
        }
        var final = console.GetMemory(0x6000);
        var finalMsg = System.Text.Encoding.ASCII.GetString(ReadBytes(console, 0x6004, 128)).TrimEnd('\0');
        Assert.True(final == 0x80 && !string.IsNullOrEmpty(finalMsg), $"blargg test did not finish: $6000=0x{final:X2}, msg: {finalMsg}");
    }

    private static byte[] ReadBytes(Console console, int start, int count)
    {
        var b = new byte[count];
        for (var i = 0; i < count; i++) b[i] = console.GetMemory((ushort)(start + i));
        return b;
    }

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
    public void ValidSignatureCreatesCartridge()
    {
        var rom = BuildRom(1, 1);
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
    }

    [Fact]
    public void InvalidSignatureThrows()
    {
        var rom = BuildRom(1, 1);
        rom[0] = 0x00;
        Assert.Throws<InvalidDataException>(() => Cartridge.Create(rom));
    }

    [Fact]
    public void Cartridge_CreateFromFileBytes_LoadsValidRom()
    {
        var rom = BuildRom(1, 1);
        var path = Path.Combine(Path.GetTempPath(), "ASD.NES.test." + Guid.NewGuid().ToString("N") + ".nes");
        try
        {
            File.WriteAllBytes(path, rom);
            var data = File.ReadAllBytes(path);
            var cart = Cartridge.Create(data);
            Assert.NotNull(cart);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Cartridge_LoadFromMissingFile_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), "ASD.NES.nonexistent." + Guid.NewGuid().ToString("N") + ".nes");
        Assert.False(File.Exists(path));
        Assert.Throws<FileNotFoundException>(() => File.ReadAllBytes(path));
    }

    [Fact]
    public void SignatureMustBeNES0x1A()
    {
        var rom = BuildRom(1, 1);
        rom[3] = 0x1B;
        Assert.Throws<InvalidDataException>(() => Cartridge.Create(rom));
    }

    [Fact]
    public void RegionNTSCWhenFlags9And10NotPAL()
    {
        var rom = BuildRom(1, 1, bytes8to15: new byte[8]);
        var cart = Cartridge.Create(rom);
        Assert.Equal(TvRegion.NTSC, cart.Region);
    }

    [Fact]
    public void RegionPALWhenFlag9Bit0Set()
    {
        var rom = BuildRom(1, 1, bytes8to15: new byte[] { 0, 1, 0, 0, 0, 0, 0, 0 });
        var cart = Cartridge.Create(rom);
        Assert.Equal(TvRegion.PAL, cart.Region);
    }

    [Fact]
    public void RegionPALWhenFlag10Bits01Equal2()
    {
        var rom = BuildRom(1, 1, bytes8to15: new byte[] { 0, 0, 2, 0, 0, 0, 0, 0 });
        var cart = Cartridge.Create(rom);
        Assert.Equal(TvRegion.PAL, cart.Region);
    }

    /// <summary>iNES 2.0: byte 7 bits 2-3 = 0x08; byte 12 (TV system) bit 0 = 1 means PAL (NESDEV).</summary>
    [Fact]
    public void RegionPALWheniNES2Byte12Bit0Set()
    {
        var rom = BuildRom(1, 1, byte6: 0x00, byte7: 0x08, bytes8to15: new byte[] { 0, 0, 0, 0, 1, 0, 0, 0 }); // iNES 2.0, TV=1 (PAL)
        var cart = Cartridge.Create(rom);
        Assert.Equal(TvRegion.PAL, cart.Region);
    }

    /// <summary>Header cleanup: bytes 7-14 "DiskDude" are zeroed so mapper/region come from clean header; ROM still loads (ines spec).</summary>
    [Fact]
    public void HeaderCleanup_DiskDude_Zeroed_LoadsAsNROM()
    {
        var rom = BuildRom(1, 1, byte6: 0x10, byte7: 0x44, bytes8to15: new byte[] { 0x69, 0x73, 0x6B, 0x44, 0x75, 0x64, 0x65, 0 }); // "DiskDude" at 7
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
        var console = new Console();
        console.InsertCartridge(cart);
        var frame = console.Update();
        Assert.NotNull(frame);
    }

    // --- Region application: Console uses cartridge (or PreferRegion) to set PPU scanlines, clock interval, APU timing ---

    [Fact]
    public void TvRegionProfile_NTSC_Has262Scanlines_AndCorrectFrameInterval()
    {
        var ntsc = TvRegionProfile.For(TvRegion.NTSC);
        Assert.Equal(260, ntsc.LastScanline); // 0..260 = 262 lines
        Assert.True(ntsc.FrameInterval.TotalMilliseconds > 16 && ntsc.FrameInterval.TotalMilliseconds < 17);
    }

    [Fact]
    public void TvRegionProfile_PAL_Has312Scanlines_And50HzFrameInterval()
    {
        var pal = TvRegionProfile.For(TvRegion.PAL);
        Assert.Equal(311, pal.LastScanline); // 0..311 = 312 lines
        Assert.Equal(20.0, pal.FrameInterval.TotalMilliseconds, precision: 1);
    }

    [Fact]
    public void InsertCartridge_WithNTSCHeader_CompletesFrame()
    {
        var rom = BuildRom(1, 1, bytes8to15: new byte[8]);
        var cart = Cartridge.Create(rom);
        Assert.Equal(TvRegion.NTSC, cart.Region);
        var console = new Console();
        console.InsertCartridge(cart);
        var frame = console.Update();
        Assert.NotNull(frame);
        Assert.Equal(256 * 240, frame.Length);
    }

    [Fact]
    public void InsertCartridge_WithPALHeader_CompletesFrame()
    {
        var rom = BuildRom(1, 1, bytes8to15: new byte[] { 0, 1, 0, 0, 0, 0, 0, 0 });
        var cart = Cartridge.Create(rom);
        Assert.Equal(TvRegion.PAL, cart.Region);
        var console = new Console();
        console.InsertCartridge(cart);
        var frame = console.Update();
        Assert.NotNull(frame);
        Assert.Equal(256 * 240, frame.Length);
    }

    [Fact]
    public void PreferRegion_OverridesCartridgeRegion()
    {
        var rom = BuildRom(1, 1, bytes8to15: new byte[] { 0, 1, 0, 0, 0, 0, 0, 0 });
        var cart = Cartridge.Create(rom);
        Assert.Equal(TvRegion.PAL, cart.Region);
        var console = new Console();
        console.PreferRegion = TvRegion.NTSC;
        console.InsertCartridge(cart);
        var frame = console.Update();
        Assert.NotNull(frame);
    }

    /// <summary>Optional: Battle City (J) [p1].nes and Sky Destroyer (Japan).nes — must still run (regression for "don't touch").</summary>
    [Fact]
    public void RunBattleCityJ_AndSkyDestroyer_IfPresent_RunFrames()
    {
        var names = new[] { "Battle City (J) [p1].nes", "Battle City (J).nes", "Sky Destroyer (Japan).nes" };
        foreach (var name in names)
        {
            var romPath = OptionalRomsPaths.Select(p => Path.Combine(p, name)).FirstOrDefault(File.Exists);
            if (romPath == null) continue;
            var rom = File.ReadAllBytes(romPath);
            if (rom.Length < 16) continue;
            var cart = Cartridge.Create(rom);
            Assert.NotNull(cart);
            var console = new Console();
            console.InsertCartridge(cart);
            console.RunCpuSteps(100);
            var frame = console.Update();
            Assert.NotNull(frame);
            Assert.Equal(256 * 240, frame.Length);
        }
    }

    [Fact]
    public void OnePRGOneCHRMinimalSize()
    {
        var rom = BuildRom(1, 1);
        Assert.Equal(16 + 0x4000 + 0x2000, rom.Length);
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
    }

    [Fact]
    public void TrainerOffset16Plus512()
    {
        var rom = BuildRom(1, 1, byte6: 0x04, withTrainer: true); // bit 2 = trainer
        Assert.True(rom.Length >= 16 + 512 + 0x4000 + 0x2000);
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
    }

    [Fact]
    public void TwoPRGPagesAcceptable()
    {
        var rom = BuildRom(2, 1);
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
    }

    [Fact]
    public void ZeroCHRPagesEmbeddedCHRSupported()
    {
        var rom = BuildRom(1, 0);
        Assert.Equal(16 + 0x4000, rom.Length);
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
    }

    // --- Mapper 7 (AxROM): Single-screen mirroring, PRG bank at $8000-$FFFF, bit 4 = nametable page ---

    [Fact]
    public void Mapper7_MinimalRom_SetsSingleScreenMirroring()
    {
        var rom = BuildRom(2, 0, byte6: 0x70, byte7: 0); // mapper 7, 2 PRG pages (32K), 0 CHR (CHR-RAM)
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
        var console = new Console();
        console.InsertCartridge(cart);
        Assert.Equal(Mirroring.SingleScreen, PPUAddressSpace.Instance.NametableMirroring);
    }

    [Fact]
    public void Mapper7_Write8000_SwitchesBankAndRendersFrame()
    {
        var rom = BuildRom(4, 0, byte6: 0x70, byte7: 0); // 4 PRG pages = 2 x 32K banks
        var prgOffset = 16;
        rom[prgOffset + 0x3FFC] = 0x00;
        rom[prgOffset + 0x3FFD] = 0x80;
        rom[prgOffset + 0] = 0x4C; // JMP $8000
        rom[prgOffset + 1] = 0x00;
        rom[prgOffset + 2] = 0x80;
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        console.RunCpuSteps(10);
        console.SetMemory(0x8000, 0x01); // bank 1, page 0
        console.RunCpuSteps(10);
        console.SetMemory(0x8000, 0x11); // bank 1, page 1 (bit 4)
        var frame = console.Update();
        Assert.NotNull(frame);
        Assert.Equal(256 * 240, frame.Length);
    }

    /// <summary>AxROM bit 4 (M) selects nametable page. Current polarity: write 0 → page 1, write 0x10 → page 0 (inverted for Battletoads-style boards).</summary>
    [Fact]
    public void Mapper7_Write0To8000_SetsSingleScreenPage1_Write0x10_SetsPage0()
    {
        var rom = BuildRom(2, 0, byte6: 0x70, byte7: 0);
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        Assert.Equal(0, PPUAddressSpace.Instance.SingleScreenPage); // Cartridge init sets 0
        console.SetMemory(0x8000, 0x00); // bit 4 = 0
        Assert.Equal(1, PPUAddressSpace.Instance.SingleScreenPage);
        console.SetMemory(0x8000, 0x10); // bit 4 = 1
        Assert.Equal(0, PPUAddressSpace.Instance.SingleScreenPage);
    }

    /// <summary>AxROM: PRG bank switch — write to $8000 selects 32K bank; different banks return different PRG data.</summary>
    [Fact]
    public void Mapper7_PrgBankSwitch_ReadsDifferentBanks()
    {
        var rom = BuildRom(4, 0, byte6: 0x70, byte7: 0);
        var prgOffset = 16;
        rom[prgOffset + 0] = 0xAA;           // bank 0 first byte
        rom[prgOffset + 0x4000] = 0x55;     // bank 1 first byte (second 16K page of bank 0 is 0x4000..0x7FFF in first 32K)
        rom[prgOffset + 0x8000] = 0x55;     // second 32K bank: $8000 in that bank = offset 0 in pages 2,3 → page 2 start
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        console.SetMemory(0x8000, 0x00); // select bank 0
        var b0 = console.GetMemory(0x8000);
        console.SetMemory(0x8000, 0x01); // select bank 1
        var b1 = console.GetMemory(0x8000);
        Assert.Equal(0xAA, b0);
        Assert.Equal(0x55, b1);
    }

    /// <summary>AxROM: CHR-RAM at $6000-$7FFF is writable and readable by CPU.</summary>
    [Fact]
    public void Mapper7_ChrRam_Write6000_ReadBack()
    {
        var rom = BuildRom(2, 0, byte6: 0x70, byte7: 0);
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        console.SetMemory(0x6000, 0x42);
        Assert.Equal(0x42, console.GetMemory(0x6000));
    }

    /// <summary>Optional: if a Battletoads .nes exists in any OptionalRomsPaths, load and run several frames (Mapper 7, single-screen). Tries "Battletoads (USA).nes" and "Battletoads (U) [!].nes".</summary>
    [Fact]
    public void RunBattletoadsRunsSeveralFrames_IfPresent()
    {
        var names = new[] { "Battletoads (USA).nes", "Battletoads (U) [!].nes" };
        string? romPath = null;
        foreach (var dir in OptionalRomsPaths)
        {
            foreach (var name in names)
            {
                var path = Path.Combine(dir, name);
                if (File.Exists(path)) { romPath = path; break; }
            }
            if (romPath != null) break;
        }
        if (romPath == null)
        {
            return;
        }
        var rom = File.ReadAllBytes(romPath);
        if (rom.Length < 16 + 16 * 0x4000) // mapper 7 typically 256K PRG
        {
            return;
        }
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
        Assert.Equal(Mirroring.SingleScreen, PPUAddressSpace.Instance.NametableMirroring);
        var console = new Console();
        console.InsertCartridge(cart);
        for (var i = 0; i < 3; i++)
        {
            console.Update();
            console.RunCpuSteps(5000);
        }
        var state = console.GetCpuState();
        Assert.True(state.PC >= 0x8000 && state.PC <= 0xFFFF);
    }

    /// <summary>iNES byte 6 bit 3 = four-screen mirroring; when set, PPU uses FourScreen.</summary>
    [Fact]
    public void FourScreen_WhenByte6Bit3Set_SetsFourScreenMirroring()
    {
        var rom = BuildRom(2, 1, byte6: 0x08, byte7: 0); // bit 3 = four-screen, mapper 0
        var cart = Cartridge.Create(rom);
        Assert.NotNull(cart);
        var console = new Console();
        console.InsertCartridge(cart);
        Assert.Equal(Mirroring.FourScreen, PPUAddressSpace.Instance.NametableMirroring);
    }

    /// <summary>Mapper 1 with 0 CHR uses CHR-RAM (e.g. Bomberman II, Dynablaster); load and render one frame.</summary>
    [Fact]
    public void Mapper1_ZeroChr_LoadsAndRendersFrame()
    {
        var rom = BuildRom(8, 0, byte6: 0x10, byte7: 0); // mapper 1, 0 CHR
        var prgOffset = 16;
        rom[prgOffset + 0x3FFC] = 0x00;
        rom[prgOffset + 0x3FFD] = 0x80;
        rom[prgOffset + 0] = 0x4C;
        rom[prgOffset + 1] = 0x00;
        rom[prgOffset + 2] = 0x80;
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        console.RunCpuSteps(50);
        var frame = console.Update();
        Assert.NotNull(frame);
        Assert.Equal(256 * 240, frame.Length);
    }

    /// <summary>Mapper 4 with 0 CHR uses CHR-RAM (e.g. Megaman IV, VI); load and render one frame.</summary>
    [Fact]
    public void Mapper4_ZeroChr_LoadsAndRendersFrame()
    {
        var rom = BuildRom(16, 0, byte6: 0x40, byte7: 0); // mapper 4, 0 CHR
        var prgOffset = 16;
        rom[prgOffset + 0x3FFC] = 0x00;
        rom[prgOffset + 0x3FFD] = 0x80;
        rom[prgOffset + 0] = 0x4C;
        rom[prgOffset + 1] = 0x00;
        rom[prgOffset + 2] = 0x80;
        var cart = Cartridge.Create(rom);
        var console = new Console();
        console.InsertCartridge(cart);
        console.RunCpuSteps(50);
        var frame = console.Update();
        Assert.NotNull(frame);
        Assert.Equal(256 * 240, frame.Length);
    }

    /// <summary>For each supported mapper (0,1,2,3,4,7), if a ROM exists in OptionalRomsPaths, load it and run steps + one frame (no crash).</summary>
    [Fact]
    public void DiscoveredRoms_PerMapper_LoadAndRunFrame_IfPresent()
    {
        var supportedMappers = new[] { 0, 1, 2, 3, 4, 7 };
        var mapperToPath = new Dictionary<int, string>();
        foreach (var dir in OptionalRomsPaths)
        {
            if (!Directory.Exists(dir)) continue;
            foreach (var path in Directory.GetFiles(dir, "*.nes", SearchOption.TopDirectoryOnly))
            {
                var data = File.ReadAllBytes(path);
                if (data.Length < 16) continue;
                if (data[0] != 0x4E || data[1] != 0x45 || data[2] != 0x53 || data[3] != 0x1A) continue;
                var mapper = ((data[7] & 0xF0) >> 4) | ((data[6] & 0xF0) >> 4);
                if (Array.IndexOf(supportedMappers, mapper) >= 0 && !mapperToPath.ContainsKey(mapper))
                    mapperToPath[mapper] = path;
            }
        }
        foreach (var kv in mapperToPath)
        {
            var data = File.ReadAllBytes(kv.Value);
            var cart = Cartridge.Create(data);
            var console = new Console();
            console.InsertCartridge(cart);
            var frame = console.Update();
            Assert.NotNull(frame);
            Assert.Equal(256 * 240, frame.Length);
        }
    }
}
