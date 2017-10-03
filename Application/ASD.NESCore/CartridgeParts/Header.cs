using System.Linq;

namespace ASD.NESCore.CartridgeParts {

    using Helpers;

    internal abstract class Header {

        public abstract DataFormat DataFormat { get; }

        public virtual int PRGROMSize { get; }
        public virtual int CHRROMSize { get; }

        public virtual int PRGRAMSize { get; }
        public virtual int PRGRAMWithBatterySize { get; }
        public virtual int CHRRAMSize { get; }
        public virtual int CHRRAMWithBatterySize { get; }

        public virtual bool HasTrainer { get; }
        public virtual bool IsVSUnisystem { get; }
        public virtual int INSTROMSize { get; }

        public virtual int MapperNumber { get; }
        public virtual int SubmapperNumber { get; }

        public virtual Mirroring Mirroring { get; }
        public virtual TvSystem TvSystem { get; }

        public virtual bool HasBusConflicts { get; }

        public static Header Create(byte[] data) {
            switch (data.GetFormat()) {
                case DataFormat.NES20: return new HeaderNES2(First16From(data));
                case DataFormat.INES: return new HeaderINES(First16From(data));
                case DataFormat.ArchNES: return new HeaderArch(First16From(data));
                default: throw new Information("Unsupported file format");
            }
            T[] First16From<T>(T[] array) => array.Take(16).ToArray();
        }

        protected Header(byte[] header) => this.header = header;
        protected byte[] header;
    }

    internal enum DataFormat { NES20, INES, ArchNES }
    internal enum Mirroring { Horizontal, Vertical, FourScreen }
    internal enum TvSystem { PAL, NTSC, DualCompatible }
}