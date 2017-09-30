namespace ASD.NESCore.ConsoleParts.CPUParts {

    /// <summary>
    /// <para>Registers are special pieces of memory in the processor
    /// which are used to carry out, and store information about calculations.</para>
    /// </summary>
    internal struct Registers {

        /// <summary>
        /// Accumulator register - arithmetic operations and exchange register.
        /// </summary>
        internal byte A;

        /// <summary>
        /// Index register X - used as a pointer offset in several indirect addressing modes.
        /// </summary>
        internal byte X;

        /// <summary>
        /// Index register Y - used as a pointer offset in several indirect addressing modes.
        /// </summary>
        internal byte Y;

        /// <summary>
        /// Program Counter register - 16-bit pointer to the currently executing piece of code.
        /// Incremented as most instructions are processed.
        /// </summary>
        internal ushort PC;

        /// <summary>
        /// Stack Pointer register - points to the current point in the stack in 256 bytes of stack space.
        /// </summary>
        internal byte SP;

        /// <summary>
        /// Processor State register
        /// </summary>
        internal byte PS;
    }
}