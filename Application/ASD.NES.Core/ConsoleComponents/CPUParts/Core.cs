using System;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    /// <summary>
    /// Emulation NMOS 6502 component of the CPU RP2A03 (Ricoh Processor 2A03)
    /// </summary>
    internal sealed class Core {

        private readonly Registers r;

        private static readonly Func<int>[] instruction = new Func<int>[] {
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 
        };

        public Core(Registers registers) {
            r = registers;
        }

        public bool HaveInstruction(ushort opcode) {
            return instruction[opcode] != null;
        }

        public int Execute(ushort opcode) {
            return 0;
        }
    }
}