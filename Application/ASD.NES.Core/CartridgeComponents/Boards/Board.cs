using System.Collections.Generic;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    using BasicComponents;

    internal abstract class Board : IMemory<byte> {

        protected IReadOnlyList<byte[]> prg;
        protected IReadOnlyList<byte[]> chr;

        public byte this[int address] {
            get => Read(address);
            set => Write(address, value);
        }
        public virtual int Cells => 1024 * 64;

        protected abstract byte Read(int address);
        protected abstract void Write(int address, byte value);

        /// <summary> PPU CHR read (0x0000-0x1FFF). Default: map to CPU 0x6000-0x7FFF. Override in MMC3 etc. </summary>
        public virtual byte ReadChr(int ppuAddress) => Read((ppuAddress & 0x1FFF) | 0x6000);
        /// <summary> PPU CHR write. Default: map to CPU 0x6000-0x7FFF. Override if CHR-RAM. </summary>
        public virtual void WriteChr(int ppuAddress, byte value) => Write((ppuAddress & 0x1FFF) | 0x6000, value);

        /// <summary> Called once per PPU scanline. MMC3 uses this for IRQ counter. </summary>
        public virtual void OnScanline() { }

        public virtual void SetCHR(IReadOnlyList<byte[]> chr) => this.chr = chr;
        public virtual void SetPRG(IReadOnlyList<byte[]> prg) => this.prg = prg;
    }
}