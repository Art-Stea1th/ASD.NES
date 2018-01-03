namespace ASD.NES.Kernel.ConsoleComponents.CPUParts {

    using Shared;
    using Registers;

    /// <summary> Registers are special pieces of memory in the processor
    /// which are used to carry out, and store information about calculations. </summary>
    internal sealed class RegistersCPU {

        /// <summary> Accumulator register - arithmetic operations and exchange register. </summary>
        internal Octet A = 0x00;

        /// <summary> Index register X - used as a pointer offset in several indirect addressing modes. </summary>
        internal Octet X = 0x00;

        /// <summary> Index register Y - used as a pointer offset in several indirect addressing modes. </summary>
        internal Octet Y = 0x00;

        /// <summary> Program Counter register - 16-bit pointer to the currently executing piece of code. </summary>
        internal Hextet PC = 0x0000;

        /// <summary> Stack Pointer register - points to the current point in the stack in 256 bytes of stack space. </summary>
        internal Octet SP = 0x00;

        /// <summary> Processor Status register </summary>
        internal readonly StateRegister PS = new StateRegister();
    }
}