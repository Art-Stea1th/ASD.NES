namespace ASD.NES.Kernel.Shared {

    /// <summary> Represents reference to 16-bit unsigned integer </summary>
    internal sealed class RefQuadlet : Reference<Quadlet> {
        public bool this[int bit] {
            get => Value[bit];
            set => Value[bit] = value;
        }
        public static RefQuadlet Wrap(Quadlet value) => new RefQuadlet(value);
        private RefQuadlet(Quadlet value) : base(value) { }

        public static implicit operator Quadlet(RefQuadlet reference) => reference.Value;
        public static implicit operator uint(RefQuadlet reference) => reference.Value;
    }

    internal sealed class RefHextet : Reference<Hextet> {
        public bool this[int bit] {
            get => Value[bit];
            set => Value[bit] = value;
        }
        public static RefHextet Wrap(Hextet value) => new RefHextet(value);
        private RefHextet(Hextet value) : base(value) { }

        public static implicit operator Hextet(RefHextet reference) => reference.Value;
        public static implicit operator ushort(RefHextet reference) => reference.Value;
    }

    /// <summary> Represents reference to 8-bit unsigned integer </summary>
    internal sealed class RefOctet : Reference<Octet> {
        public bool this[int bit] {
            get => Value[bit];
            set => Value[bit] = value;
        }
        public static RefOctet Wrap(Octet value) => new RefOctet(value);
        private RefOctet(Octet value) : base(value) { }

        public static implicit operator Octet(RefOctet reference) => reference.Value;
        public static implicit operator byte(RefOctet reference) => reference.Value;
    }

    internal abstract class Reference<TValue> where TValue : struct {

        public TValue Value;

        protected Reference(TValue value = default(TValue))
            => Value = value;
    }
}