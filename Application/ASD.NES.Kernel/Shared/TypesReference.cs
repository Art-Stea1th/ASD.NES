namespace ASD.NES.Kernel.Shared {

    using Helpers;

    /// <summary> Represents reference to 32-bit unsigned integer </summary>
    internal sealed class RefInt32 {

        public uint Value { get; set; }

        public bool this[int bit] {
            get => Value.HasBit(bit);
            set => Value = Value.WithChangedBit(bit, value);
        }
        public static RefInt32 Wrap(uint value) => new RefInt32(value);
        private RefInt32(uint value)
            => Value = value;

        public static implicit operator uint(RefInt32 reference) => reference.Value;
    }

    /// <summary> Represents reference to 16-bit unsigned integer </summary>
    internal sealed class RefInt16 {

        public ushort Value { get; set; }

        public bool this[int bit] {
            get => Value.HasBit(bit);
            set => Value = Value.WithChangedBit(bit, value);
        }
        public static RefInt16 Wrap(ushort value) => new RefInt16(value);
        private RefInt16(ushort value)
            => Value = value;

        public static implicit operator ushort(RefInt16 reference) => reference.Value;
    }

    /// <summary> Represents reference to 8-bit unsigned integer </summary>
    internal sealed class RefInt8 {

        public byte Value { get; set; }

        public bool this[int bit] {
            get => Value.HasBit(bit);
            set => Value = Value.WithChangedBit(bit, value);
        }
        public static RefInt8 Wrap(byte value) => new RefInt8(value);
        private RefInt8(byte value)
            => Value = value;

        public static implicit operator byte(RefInt8 reference) => reference.Value;
    }
}