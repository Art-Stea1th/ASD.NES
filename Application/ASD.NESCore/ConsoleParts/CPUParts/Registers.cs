namespace ASD.NESCore.ConsoleParts.CPUParts {

    using Common;

    /// <summary>
    /// <para>Registers are special pieces of memory in the processor
    /// which are used to carry out, and store information about calculations.</para>
    /// </summary>
    internal struct Registers {

        /// <summary>
        /// Accumulator register - arithmetic operations and exchange register.
        /// </summary>
        internal RInt8 A;

        /// <summary>
        /// Index register X - used as a pointer offset in several indirect addressing modes.
        /// </summary>
        internal RInt8 X;

        /// <summary>
        /// Index register Y - used as a pointer offset in several indirect addressing modes.
        /// </summary>
        internal RInt8 Y;

        /// <summary>
        /// Program Counter register - 16-bit pointer to the currently executing piece of code.
        /// Incremented as most instructions are processed.
        /// </summary>
        internal RInt16 PC;

        /// <summary>
        /// Stack Pointer register - points to the current point in the stack in 256 bytes of stack space.
        /// </summary>
        internal RInt8 SP;

        /// <summary>
        /// Processor State register
        /// </summary>
        internal RInt8 PS;
    }
}