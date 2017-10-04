namespace ASD.NESCore.ConsoleParts {

    using Common;
    using CPUParts;

    /// <summary>
    /// Emulation of the CPU RP2A03 (Ricoh Processor 2A03)
    /// </summary>
    internal sealed class CentralProcessor {

        private static readonly MemoryBus bus = MemoryBus.Instance;

        private RInt8[] zeroPage, stack, wram;
        private RInt8 res, nmi, irq, brk;

        private Core core;

        private void Initialize() {

            zeroPage = bus.GetReferenceRange(0, 0x100);
            stack = bus.GetReferenceRange(0x100, 0x100);
            wram = bus.GetReferenceRange(0x200, 0x600);

            res = bus.GetReference(0xFFFC);
            nmi = bus.GetReference(0xFFFA);
            irq = brk = bus.GetReference(0xFFFE);
        }
    }
}