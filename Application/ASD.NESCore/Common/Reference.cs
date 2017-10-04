namespace ASD.NESCore.Common {

    using Helpers;

    internal sealed class RInt16 : Reference<ushort> {

        //public bool this[int bit] {
        //    get => Value.HasBit(bit);
        //    set => Value = Value.SetBit(bit, value);
        //}

        public static RInt16 Wrap(ushort value) => new RInt16(value);
        private RInt16(ushort value) : base(value) { }

        public static implicit operator ushort(RInt16 reference) => reference.Value;
    }

    internal sealed class RInt8 : Reference<byte> {

        public bool this[int bit] {
            get => Value.HasBit(bit);
            set => Value = Value.SetBit(bit, value);
        }

        public static RInt8 Wrap(byte value) => new RInt8(value);
        private RInt8(byte value) : base(value) { }

        public static implicit operator byte(RInt8 reference) => reference.Value;
    }

    internal class Reference<T> where T : struct {
        protected Reference(T value = default(T)) => Value = value;
        public T Value;
    }
}