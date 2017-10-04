using System;

namespace ASD.NESCore.ConsoleParts.CPUParts {

    /// <summary>
    /// Emulation NMOS 6502 component of the CPU RP2A03 (Ricoh Processor 2A03)
    /// </summary>
    internal sealed class Core {

        private Registers register;

        private Action[] instruction;
    }
}