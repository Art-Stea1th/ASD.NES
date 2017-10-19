using System.Collections.Generic;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    using BasicComponents;
    using Shared;

    internal abstract class Board : IMemory<Octet> {

        public Octet this[int address] {
            get => Read(address);
            set => Write(address, value);
        }
        public abstract int Cells { get; }

        protected abstract Octet Read(int address);
        protected abstract void Write(int address, Octet value);

        public abstract void SetCHR(IReadOnlyList<Octet[]> chr);
        public abstract void SetPRG(IReadOnlyList<Octet[]> prg);
    }
}