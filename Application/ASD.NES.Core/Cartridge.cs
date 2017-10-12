using System;
using System.IO;
using System.Linq;
using OldCode;

namespace ASD.NES.Core {

    using Helpers;

    public sealed class Cartridge {

        private int PRGROM { get; set; }
        private int CHRROM { get; set; }

        static readonly uint iNES_FILE_SIGNATURE = 0x1A53454E;

        public static Cartridge Create(byte[] data) => new Cartridge(data);

        private Cartridge(byte[] data) {

            byte[] header = data.Take(16).ToArray();

            var fileSignature = BitConverter.ToUInt32(header, 0);
            if (fileSignature != iNES_FILE_SIGNATURE) {
                throw new InvalidDataException();
            }

            var mapperNumder = BitOperations.MakeInt8(header[7].HNybble(), header[6].HNybble());
            if (mapperNumder != 0) {
                throw new Information($"Mapper {mapperNumder} - Unsupported Mapper. Only Mapper 0 is supported.");
            }

            PRGROM = header[4];
            CHRROM = header[5];

            OldMemoryBus.Instance.NametableMirroring
                = header[6].HasBit(1)
                ? NametableMirroringMode.Vertical
                : NametableMirroringMode.Horizontal;

            var prgStart = 16;
            var prgBytes = 0x4000 * PRGROM;
            OldMemoryBus.Instance.SetPRG0(data.Skip(prgStart).Take(prgBytes).ToArray());

            var chrStart = prgStart + prgBytes;
            var chrBytes = 0x2000 * CHRROM;
            OldMemoryBus.Instance.SetCHR(data.Skip(chrStart).Take(chrBytes).ToArray());
        }
    }
}