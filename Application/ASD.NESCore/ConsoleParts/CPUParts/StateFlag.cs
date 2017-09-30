namespace ASD.NESCore.ConsoleParts.CPUParts {

    /// <summary>
    /// Processor State flag
    /// </summary>
    internal enum StateFlag : byte {

        /// <summary>
        /// "All" - for bit operations.<para/>
        /// bit: all set, hex: 0xFF, dec: 255
        /// </summary>
        All = 0b1111_1111,

        /// <summary>
        /// "Sign" - set when the previous operation resulted in a negative value.<para/>
        /// bit: 7 set, hex: 0x80, dec: 128
        /// </summary>
        S = 0b1000_0000,

        /// <summary>
        /// "Overflow" - set when the previous caused a signed overflow.<para/>
        /// bit: 6 set, hex: 0x40, dec: 64
        /// </summary>
        V = 0b0100_0000,

        /// <summary>
        /// "Unused" - not used. Supposed to be logical 1 at all times.<para/>
        /// bit: 5 set, hex: 0x20, dec: 32
        /// </summary>
        U = 0b0010_0000,

        /// <summary>
        /// "Break" - set when a software interrupt (BRK instruction) is executed.<para/>
        /// bit: 4 set, hex: 0x10, dec: 16
        /// </summary>
        B = 0b0001_0000,

        /// <summary>
        /// "Decimal" - set when the Decimal Mode is enabled.<para/>
        /// bit: 3 set, hex: 0x08, dec: 8
        /// </summary>
        D = 0b0000_1000,

        /// <summary>
        /// "Interrupt disable" - set: only NMI interrupts will get through (unset: IRQ and NMI will get through)<para/>
        /// bit: 2 set, hex: 0x04, dec: 4
        /// </summary>
        I = 0b0000_0100,

        /// <summary>
        /// "Zero" - set when the last operation resulted in a zero.<para/>
        /// bit: 1 set, hex: 0x02, dec: 2
        /// </summary>
        Z = 0b0000_0010,

        /// <summary>
        /// "Carry" - set when the last addition or shift resulted in a carry, or last subtraction resulted in no borrow.<para/>
        /// bit: 0 set, hex: 0x01, dec: 1
        /// </summary>
        C = 0b0000_0001,

        /// <summary>
        /// "Empty" - for bit operations.<para/>
        /// bit: all unset, hex: 0x00, dec: 0
        /// </summary>
        Empty = 0b0000_0000
    }
}