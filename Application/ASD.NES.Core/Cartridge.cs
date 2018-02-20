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

        public static Cartridge Create(byte[] data) => new Cartridge(data);

        private Cartridge(byte[] data) {

            var header = data.Take(16).ToArray();

            var fileSignature = BitConverter.ToUInt32(header.Select(o => o).ToArray(), 0);
            if (fileSignature != iNesFileSignature) {
                throw new InvalidDataException();
            }

            var mapperNumder = BitOperations.MakeInt8(header[7].H(), header[6].H());
            switch (mapperNumder) {
                case 0: board = new Mapper000(); break;
                case 3: board = new Mapper003(); break;
                default: throw new Information($"Mapper {mapperNumder} - Unsupported.");
            }

            PRGCount = header[4];
            CHRCount = header[5];

            PPUAddressSpace.Instance.NametableMirroring
                = header[6].HasBit(1)
                ? Mirroring.Vertical
                : Mirroring.Horizontal;

            var prgStart = 16;
            var prgBytes = 0x4000 * PRGCount;

            var chrStart = prgStart + prgBytes;
            var chrBytes = 0x2000 * CHRCount;

            prg = new List<byte[]>(PRGCount);
            for (var i = 0; i < PRGCount; i++) {
                var offset = prgStart + 0x4000 * i;
                prg.Add(data.Skip(prgStart).Take(0x4000).ToArray());
            }

            chr = new List<byte[]>(CHRCount);
            for (var i = 0; i < CHRCount; i++) {
                var offset = chrStart + 0x2000 * i;
                chr.Add(data.Skip(chrStart).Take(0x2000).ToArray());
            }

            board.SetCHR(chr);
            board.SetPRG(prg);

            CPUAddressSpace.Instance.SetExternalMemory(board);
            PPUAddressSpace.Instance.SetExternalMemory(board);
        }
    }
}