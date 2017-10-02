namespace ASD.NESCore.Common {

    using Helpers;

    internal sealed class RByte : Reference<byte> {

        public bool this[int bit] {
            get => Value.IsSetBit(bit);
            set => Value = Value.SetBit(bit, value);
        }

        public static RByte Wrap(byte value) => new RByte(value);
        private RByte(byte value) : base(value) { }

        public static implicit operator byte(RByte rByte) => rByte.Value;
    }

    internal class Reference<T> where T : struct {
        protected Reference(T value = default(T)) => Value = value;
        public T Value;
    }
}