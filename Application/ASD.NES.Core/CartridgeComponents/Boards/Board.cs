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

        public virtual void SetCHR(IReadOnlyList<byte[]> chr) => this.chr = chr;
        public virtual void SetPRG(IReadOnlyList<byte[]> prg) => this.prg = prg;
    }
}