namespace ASD.NES.Core.Shared {

    using Helpers;

    internal class RInt8 : Reference<byte> {

        public bool this[int bit] {
            get => Value.HasBit(bit);
            set => Value = Value.WithChangedBit(bit, value);
        }
        public static RInt8 Wrap(byte value) => new RInt8(value);
        protected RInt8(byte value) : base(value) { }

        public static implicit operator byte(RInt8 reference) => reference.Value;        
    }
}