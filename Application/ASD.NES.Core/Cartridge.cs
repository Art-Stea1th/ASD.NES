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

        public static Cartridge Create(byte[] data) => new Cartridge(data);

        private Cartridge(byte[] data) {

            var header = data.Take(16).ToArray();

            var fileSignature = BitConverter.ToUInt32(header.Select(o => o).ToArray(), 0);
            if (fileSignature != iNesFileSignature) {
                throw new InvalidDataException();
            }

            // iNES Flags 9: bit 0 = TV system (0: NTSC; 1: PAL). Flags 10 (unofficial): bits 0-1 = 2 means PAL.
            var palFrom9 = header.Length > 9 && (header[9] & 1) != 0;
            var palFrom10 = header.Length > 10 && (header[10] & 3) == 2;
            Region = (palFrom9 || palFrom10) ? TvRegion.PAL : TvRegion.NTSC;

            var mapperNumber = BitOperations.MakeInt8(header[7].H(), header[6].H());
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

            PRGCount = header[4];
            CHRCount = header[5];

            // iNES byte 6: bit 0 = mirroring (0=horizontal, 1=vertical). Many NROM dumps use bit 1; use bit 0 for mappers 1+, bit 1 for mapper 0 for compatibility.
            var ppu = PPUAddressSpace.Instance;
            if (mapperNumber == 7) {
                ppu.NametableMirroring = Mirroring.SingleScreen;
                ppu.SingleScreenPage = 0;
            }
            else {
                var mirrorVertical = mapperNumber == 0 ? header[6].HasBit(1) : header[6].HasBit(0);
                ppu.NametableMirroring = mirrorVertical ? Mirroring.Vertical : Mirroring.Horizontal;
            }

            var hasTrainer = header[6].HasBit(2);
            var prgStart = 16 + (hasTrainer ? 512 : 0);
            var chrStart = prgStart + 0x4000 * PRGCount;

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

            CPUAddressSpace.Instance.SetExternalMemory(board);
            PPUAddressSpace.Instance.SetExternalMemory(board);
        }
    }
}