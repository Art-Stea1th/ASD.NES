namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    using Helpers;

    /// <summary>
    /// Processor State flag
    /// </summary>
    internal sealed class StateFlag {

        private byte flag = 0x00;

        public void SetNew(byte flag) => this.flag = flag;

        public static implicit operator byte(StateFlag state) => state.flag;

        /// <summary>
        /// "Sign" - set when the previous operation resulted in a negative value.<para/>
        /// bit: 7 set, hex: 0x80, dec: 128
        /// </summary>
        public bool S { get => flag.HasBit(7); set => flag = flag.WithChangedBit(7, value); }

        /// <summary>
        /// "Overflow" - set when the previous caused a signed overflow.<para/>
        /// bit: 6 set, hex: 0x40, dec: 64
        /// </summary>
        public bool V { get => flag.HasBit(6); set => flag = flag.WithChangedBit(6, value); }

        /// <summary>
        /// "Unused" - not used. Supposed to be logical 1 at all times.<para/>
        /// bit: 5 set, hex: 0x20, dec: 32
        /// </summary>
        public bool U { get => flag.HasBit(5); set => flag = flag.WithChangedBit(5, value); }

        /// <summary>
        /// "Break" - set when a software interrupt (BRK instruction) is executed.<para/>
        /// bit: 4 set, hex: 0x10, dec: 16
        /// </summary>
        public bool B { get => flag.HasBit(4); set => flag = flag.WithChangedBit(4, value); }

        /// <summary>
        /// "Decimal" - set when the Decimal Mode is enabled.<para/>
        /// bit: 3 set, hex: 0x08, dec: 8
        /// </summary>
        public bool D { get => flag.HasBit(3); set => flag = flag.WithChangedBit(3, value); }

        /// <summary>
        /// "Interrupt disable" - set: only NMI interrupts will get through (unset: IRQ and NMI will get through)<para/>
        /// bit: 2 set, hex: 0x04, dec: 4
        /// </summary>
        public bool I { get => flag.HasBit(2); set => flag = flag.WithChangedBit(2, value); }

        /// <summary>
        /// "Zero" - set when the last operation resulted in a zero.<para/>
        /// bit: 1 set, hex: 0x02, dec: 2
        /// </summary>
        public bool Z { get => flag.HasBit(1); set => flag = flag.WithChangedBit(1, value); }

        /// <summary>
        /// "Carry" - set when the last addition or shift resulted in a carry, or last subtraction resulted in no borrow.<para/>
        /// bit: 0 set, hex: 0x01, dec: 1
        /// </summary>
        public bool C { get => flag.HasBit(0); set => flag = flag.WithChangedBit(0, value); }
    }
}