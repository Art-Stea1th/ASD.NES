using System.Linq;

namespace ASD.NESCore.CartridgeParts {

    using Helpers;

    internal enum FileType { NES20, INES, ArchNES }
    internal enum Mirroring { Horizontal, Vertical, FourScreen }

    internal sealed class NES2Header : Header {
        public override FileType FileType => FileType.NES20;
        public NES2Header(byte[] header) : base(header) { }

    }

    internal sealed class NESHeader : Header {
        public override FileType FileType => FileType.INES;
        public NESHeader(byte[] header) : base(header) { }

    }

    internal sealed class ArchNESHeader : Header {
        public override FileType FileType => FileType.ArchNES;
        public ArchNESHeader(byte[] header) : base(header) { }

    }

    internal abstract class Header {

        private byte[] header;

        public abstract FileType FileType { get; }

        public bool ContainsPRGROM => PRGROMCount > 0;
        public bool ContainsCHRROM => CHRROMCount > 0;
        public bool ContainsPRGRAM => header[6].IsSetBit(1);
        public bool ContainsCHRRAM => !ContainsCHRROM;

        public int PRGROMCount => header[4];
        public int CHRROMCount => header[5];        

        public Mirroring Mirroring => header[6].IsSetBit(3)
            ? Mirroring.FourScreen
            : header[6].IsSetBit(0)
            ? Mirroring.Vertical
            : Mirroring.Horizontal;

        public bool ContainsTrainer => header[6].IsSetBit(2); // 512-byte at $7000-$71FF (stored before PRG data)
        public bool IsPlayChoice => header[7].IsSetBit(1);    // 8KB of Hint Screen data after CHR data

        public byte MapperNumber => BitOperations.MakeInt8(header[7].HigBits(), header[6].HigBits());

        public static Header Create(byte[] data) {
            var fileType = data.GetFileType();
            switch (fileType) {
                case FileType.NES20: return new NES2Header(First16From(data));
                case FileType.INES: return new NESHeader(First16From(data));
                case FileType.ArchNES: return new ArchNESHeader(First16From(data));
                default: throw new Information("Invalid file data");
            }
            T[] First16From<T>(T[] array) => array.Take(16).ToArray();
        }

        protected Header(byte[] header) => this.header = header;
    }

    internal static class LocalExtensions {

        public static bool ContainsNESLabel(this byte[] data)
            => new string(data.Take(3).Select(c => (char)c).ToArray()) == "NES";

        public static FileType GetFileType(this byte[] data) {

            var expectedPRG = BitOperations.MakeInt16(data[9].LowBits(), data[4]);
            var expectedCHR = BitOperations.MakeInt16(data[9].HigBits(), data[5]);

            if (data[7].IsSetBit(3) && expectedPRG < data.Length) {
                return FileType.NES20;
            }
            if (!data[7].IsSetBit(3) & data[12] == 0 & data[13] == 0 & data[14] == 0 & data[15] == 0) {
                return FileType.INES;
            }
            return FileType.ArchNES;
        }
    }
}