﻿namespace ASD.NES.Core.Shared {

    internal sealed class RInt16 : Reference<ushort> {

        public static RInt16 Wrap(ushort value) => new RInt16(value);
        private RInt16(ushort value) : base(value) { }

        public static implicit operator ushort(RInt16 reference) => reference.Value;
    }
}