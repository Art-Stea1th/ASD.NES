using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ASD.NES.Core {

    using CartridgeComponents.Boards;
    using CartridgeComponents.Mappers;
    using ConsoleComponents.CPUParts;
    using ConsoleComponents.PPUParts;
    using Helpers;

    public sealed class Cartridge { // hardcode read

        private Board board;

        private int PRGCount { get; set; }
        private int CHRCount { get; set; }

        private static List<byte[]> prg;
        private static List<byte[]> chr;

        private static readonly uint iNesFileSignature = 0x1A53454E;

        /// <summary> TV region from iNES header: Flags 9 bit 0 (0=NTSC, 1=PAL), or Flags 10 bits 0-1 (2=PAL). Default NTSC if not specified. </summary>
        public TvRegion Region { get; }

        /// <summary> Reset vector from PRG (0xFFFC/0xFFFD) for tests. </summary>
        internal ushort ResetVector { get; private set; }

        public static Cartridge Create(byte[] data) => new Cartridge(data);

        private Cartridge(byte[] data) {

            var header = data.Take(16).ToArray();

            var fileSignature = BitConverter.ToUInt32(header.Select(o => o).ToArray(), 0);
            if (fileSignature != iNesFileSignature) {
                throw new InvalidDataException();
            }

            CleanupHeader(header);

            // iNES 2.0: (byte 7 & 0x0C) == 0x08 (NESDEV).
            var isNes2 = header.Length > 8 && (header[7] & 0x0C) == 0x08;

            // Region: iNES 1.0 Flags 9 (bit 0), Flags 10 (bits 0-1 == 2); iNES 2.0 byte 12 (TV system, bit 0 = PAL).
            var palFrom9 = header.Length > 9 && (header[9] & 1) != 0;
            var palFrom10 = header.Length > 10 && (header[10] & 3) == 2;
            var palFrom12 = isNes2 && header.Length > 12 && (header[12] & 1) != 0;
            Region = (palFrom9 || palFrom10 || palFrom12) ? TvRegion.PAL : TvRegion.NTSC;

            // Mapper: low from byte 6|7; iNES 2.0 upper bits from byte 8 (NESDEV).
            var mapperNumber = (int)BitOperations.MakeInt8(header[7].H(), header[6].H());
            if (isNes2 && header.Length > 8)
                mapperNumber |= (header[8] & 0x0F) << 8;

            switch (mapperNumber) {
                case 0: board = new Mapper000(); break;
                case 1: board = new Mapper001(); break;
                case 2: board = new Mapper002(); break;
                case 3: board = new Mapper003(); break;
                case 4: board = new Mapper004(); break;
                case 7: board = new Mapper007(); break;
                case 11: board = new Mapper011(); break;
                case 34: board = new Mapper034(); break;
                case 66: board = new Mapper066(); break;
                case 71: board = new Mapper071(); break;
                case 79: board = new Mapper079(); break;
                default: board = new Mapper000(); break; // fallback: use NROM so ROM loads without crash
            }

            // PRG/CHR sizes: iNES 2.0 upper bits in byte 9 (NESDEV).
            PRGCount = header[4];
            CHRCount = header[5];
            if (isNes2 && header.Length > 9) {
                PRGCount |= (header[9] & 0x0F) << 8;
                CHRCount |= (header[9] & 0xF0) << 4;
            }
            if (PRGCount == 0) PRGCount = 1; // avoid empty PRG

            // iNES byte 6: bit 0 = mirroring, bit 3 = four-screen. Mapper 7 uses SingleScreen.
            var ppu = PPUAddressSpace.Instance;
            if (mapperNumber == 7) {
                ppu.NametableMirroring = Mirroring.SingleScreen;
                ppu.SingleScreenPage = 0;
            }
            else if (header[6].HasBit(3)) {
                ppu.NametableMirroring = Mirroring.FourScreen;
            }
            else {
                var mirrorVertical = mapperNumber == 0 ? header[6].HasBit(1) : header[6].HasBit(0);
                ppu.NametableMirroring = mirrorVertical ? Mirroring.Vertical : Mirroring.Horizontal;
            }

            var hasTrainer = header[6].HasBit(2);
            var prgStart = 16 + (hasTrainer ? 512 : 0);
            var maxPrgByFile = Math.Max(0, (data.Length - prgStart) / 0x4000);
            if (PRGCount > maxPrgByFile) PRGCount = maxPrgByFile;
            if (PRGCount == 0) PRGCount = 1;

            var chrStart = prgStart + 0x4000 * PRGCount;
            var maxChrByFile = Math.Max(0, (data.Length - chrStart) / 0x2000);
            if (CHRCount > maxChrByFile) CHRCount = maxChrByFile;

            prg = new List<byte[]>(PRGCount);
            for (var i = 0; i < PRGCount; i++) {
                var offset = prgStart + 0x4000 * i;
                prg.Add(data.Skip(offset).Take(0x4000).ToArray());
            }

            chr = new List<byte[]>(CHRCount);
            for (var i = 0; i < CHRCount; i++) {
                var offset = chrStart + 0x2000 * i;
                chr.Add(data.Skip(offset).Take(0x2000).ToArray());
            }

            board.SetCHR(chr);
            board.SetPRG(prg);

            ResetVector = PRGCount > 0
                ? (ushort)(prg[0][0x3FFC] | (prg[0][0x3FFD] << 8))
                : (ushort)0;

            CPUAddressSpace.Instance.SetExternalMemory(board);
            PPUAddressSpace.Instance.SetExternalMemory(board);
        }

        /// <summary> Zero known garbage in header bytes 7-15 so mapper/region/sizes are correct (ines header cleanup). </summary>
        private static void CleanupHeader(byte[] header) {
            if (header == null || header.Length < 16) return;
            // "DiskDude" at 7 (8 chars) or "demiforce" at 7 (9 chars) → zero 7..15
            if (Matches(header, 7, "DiskDude") || Matches(header, 7, "demiforce")) {
                for (var i = 7; i < 16; i++) header[i] = 0;
                return;
            }
            // "Ni03" at 10: if "Dis" at 7 zero 7..15, else zero 10..15
            if (Matches(header, 10, "Ni03")) {
                if (Matches(header, 7, "Dis"))
                    for (var i = 7; i < 16; i++) header[i] = 0;
                else
                    for (var i = 10; i < 16; i++) header[i] = 0;
            }
        }

        private static bool Matches(byte[] h, int start, string s) {
            if (h == null || start + s.Length > h.Length) return false;
            for (var i = 0; i < s.Length; i++)
                if (h[start + i] != (byte)s[i]) return false;
            return true;
        }
    }
}