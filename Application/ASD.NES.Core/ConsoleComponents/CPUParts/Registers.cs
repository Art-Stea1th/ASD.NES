namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    /// <summary>
    /// <para>Registers are special pieces of memory in the processor
    /// which are used to carry out, and store information about calculations.</para>
    /// </summary>
    internal class Registers {

        /// <summary>
        /// Accumulator register - arithmetic operations and exchange register.
        /// </summary>
        internal byte A = 0x00;

        /// <summary>
        /// Index register X - used as a pointer offset in several indirect addressing modes.
        /// </summary>
        internal byte X = 0x00;

        /// <summary>
        /// Index register Y - used as a pointer offset in several indirect addressing modes.
        /// </summary>
        internal byte Y = 0x00;

        /// <summary>
        /// Program Counter register - 16-bit pointer to the currently executing piece of code.
        /// Incremented as most instructions are processed.
        /// </summary>
        internal ushort PC = 0x0000;

        /// <summary>
        /// Stack Pointer register - points to the current point in the stack in 256 bytes of stack space.
        /// </summary>
        internal byte SP = 0x00;

        /// <summary>
        /// Processor State register
        /// </summary>
        internal readonly StateFlag PS = new StateFlag();
    }
}