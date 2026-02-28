namespace ASD.NES.Core.ConsoleComponents.CPUParts.Registers {

    using Helpers;

    /// <summary> Processor State flag </summary>
    internal sealed class StateRegister {

        private byte r;

        /// <summary> "Signed" (bit 7) - set when the previous operation resulted in a negative value. </summary>
        public bool S { get => r.HasBit(7); set => r = r.WithChangedBit(7, value); }

        /// <summary> "Overflow" (bit 6) - set when the previous caused a signed overflow. </summary>
        public bool V { get => r.HasBit(6); set => r = r.WithChangedBit(6, value); }

        /// <summary> "Unused" (bit 5) - not used. Supposed to be logical 1 at all times. </summary>
        public bool U { get => r.HasBit(5); set => r = r.WithChangedBit(5, value); }

        /// <summary> "Break" (bit 4) - set when a software interrupt (BRK instruction) is executed. </summary>
        public bool B { get => r.HasBit(4); set => r = r.WithChangedBit(4, value); }

        /// <summary> "Decimal" (bit 3) - set when the Decimal Mode is enabled. </summary>
        public bool D { get => r.HasBit(3); set => r = r.WithChangedBit(3, value); }

        /// <summary> "Interrupt disable" (bit 2) - set: only NMI interrupts will get through (unset: IRQ and NMI will get through) </summary>
        public bool I { get => r.HasBit(2); set => r = r.WithChangedBit(2, value); }

        /// <summary> "Zero" (bit 1) - set when the last operation resulted in a zero. </summary>
        public bool Z { get => r.HasBit(1); set => r = r.WithChangedBit(1, value); }

        /// <summary> "Carry" (bit 0) - set when the last addition or shift resulted in a carry, or last subtraction resulted in no borrow. </summary>
        public bool C { get => r.HasBit(0); set => r = r.WithChangedBit(0, value); }

        /// <summary> Keep legacy int-based methods but forward to byte-aware implementations
        public void UpdateSigned(int value) => UpdateSigned((byte)value);
        public void UpdateSigned(byte value) => S = (value & 0x80) != 0;

        /// <summary> Overflow depends on operands (A, M) and result; use specific helpers for add/sub
        /// <summary> Legacy API kept as no-op to avoid breaking callers — use UpdateOverflowAdd/UpdateOverflowSub from CPUCore.
        public void UpdateOverflow(int value) => V = false;

        public void UpdateZero(int value) => UpdateZero((byte)value);
        public void UpdateZero(byte value) => Z = value == 0;

        public void UpdateCarry(int value) => UpdateCarryAdd(value);
        public void UpdateCarryAdd(int result) => C = (result & 0x100) != 0;

        /// <summary> Proper overflow helpers
        public void UpdateOverflowAdd(byte a, byte m, byte result) {
            // Overflow for addition: (~(A ^ M) & (A ^ R)) & 0x80
            V = (((~(a ^ m)) & (a ^ result)) & 0x80) != 0;
        }

        public void UpdateOverflowSub(byte a, byte m, byte result) {
            // Overflow for subtraction: ((A ^ M) & (A ^ R)) & 0x80
            V = (((a ^ m) & (a ^ result)) & 0x80) != 0;
        }

        public void SetNew(byte vatue) => r = vatue;
        public static implicit operator byte(StateRegister state) => state.r;
    }
}