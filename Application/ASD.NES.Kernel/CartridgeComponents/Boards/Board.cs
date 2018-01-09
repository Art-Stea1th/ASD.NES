using System.Collections.Generic;

namespace ASD.NES.Kernel.CartridgeComponents.Boards {

    using BasicComponents;

    internal abstract class Board : IMemory<byte> {

        public byte this[int address] {
            get => Read(address);
            set => Write(address, value);
        }
        public abstract int Cells { get; }

        protected abstract byte Read(int address);
        protected abstract void Write(int address, byte value);

        public abstract void SetCHR(IReadOnlyList<byte[]> chr);
        public abstract void SetPRG(IReadOnlyList<byte[]> prg);
    }
}