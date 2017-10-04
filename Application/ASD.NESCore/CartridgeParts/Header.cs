using System.Linq;

namespace ASD.NESCore.CartridgeParts {

    using Helpers;

    internal abstract class Header {

        public abstract DataFormat DataFormat { get; }

        public virtual int PRGROMs { get; }
        public virtual int CHRROMs { get; }

        public virtual int PRGRAMs { get; }
        public virtual int PRGRAMsWithBattery { get; }
        public virtual int CHRRAMs { get; }
        public virtual int CHRRAMsWithBattery { get; }

        public virtual bool HasTrainer { get; }
        public virtual bool HasVSUnisystem { get; }
        public virtual int HasPlayChoice { get; }

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