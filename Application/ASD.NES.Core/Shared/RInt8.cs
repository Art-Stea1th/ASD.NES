namespace ASD.NES.Core.Shared {

    using Helpers;

    internal sealed class RInt8 : Reference<byte> {

        public bool this[int bit] {
            get => Value.HasBit(bit);
            set => Value = Value.WithChangedBit(bit, value);
        }

        public static RInt8 Wrap(byte value) => new RInt8(value);
        private RInt8(byte value) : base(value) { }

        public static implicit operator byte(RInt8 reference) => reference.Value;
    }
}
