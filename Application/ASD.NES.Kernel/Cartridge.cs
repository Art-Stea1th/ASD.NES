using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ASD.NES.Kernel {

    using CartridgeComponents.Boards;
    using CartridgeComponents.Mappers;
    using ConsoleComponents.CPUParts;
    using ConsoleComponents.PPUParts;
    using Helpers;
    using Shared;

    public sealed class Cartridge { // hardcode read

        private int PRGCount { get; set; }
        private int CHRCount { get; set; }

        private Board board;

        private static readonly uint iNesFileSignature = 0x1A53454E;

        public static Cartridge Create(byte[] data) => new Cartridge(data);

        private Cartridge(byte[] data) {

            Octet[] header = data.Take(16).Select(b => (Octet)b).ToArray();

            var fileSignature = BitConverter.ToUInt32(header.Select(o => (byte)o).ToArray(), 0);
            if (fileSignature != iNesFileSignature) {
                throw new InvalidDataException();
            }

            var mapperNumder = Octet.Make(header[7].H, header[6].H);
            if (mapperNumder != 0) {
                throw new Information($"Mapper {(byte)mapperNumder} - Unsupported Mapper. Only Mapper 0 is supported.");
            }

            PRGCount = header[4];
            CHRCount = header[5];

            PPUAddressSpace.Instance.NametableMirroring
                = header[6][1]
                ? Mirroring.Vertical
                : Mirroring.Horizontal;

            var prgStart = 16;
            var prgBytes = 0x4000 * PRGCount;

            var chrStart = prgStart + prgBytes;
            var chrBytes = 0x2000 * CHRCount;

            var prgList = new List<Octet[]>(PRGCount);
            for (var i = 0; i < PRGCount; i++) {
                var offset = prgStart + 0x4000 * i;
                prgList.Add(data.Skip(prgStart).Take(0x4000).Select(b => (Octet)b).ToArray());
            }

            var chrList = new List<Octet[]>(CHRCount);
            for (var i = 0; i < CHRCount; i++) {
                var offset = chrStart + 0x2000 * i;
                chrList.Add(data.Skip(chrStart).Take(0x2000).Select(b => (Octet)b).ToArray());
            }

            board = new Mapper000();
            board.SetCHR(chrList);
            board.SetPRG(prgList);

            CPUAddressSpace.Instance.SetExternalMemory(board);
            PPUAddressSpace.Instance.SetExternalMemory(board);
        }
    }
}