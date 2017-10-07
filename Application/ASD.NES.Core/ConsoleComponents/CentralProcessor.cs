using System;
using System.Collections.Generic;
using System.Text;

using OldCode;

namespace ASD.NES.Core.ConsoleComponents {

    using Shared;
    using CPUParts;

    internal sealed class CentralProcessor {

        // -------------
        private OldCPU oldCpu;
        // -------------

        private static readonly OldMemoryBus bus = OldMemoryBus.Instance;

        private RInt8[] zeroPage, stack, wram;
        private RInt8 res, nmi, irq, brk;

        private Core core;
        private Registers registers;

        public CentralProcessor() {
            Initialize();
        }

        

        private void Initialize() {

            zeroPage = bus.GetReferenceRange(0, 0x100);
            stack = bus.GetReferenceRange(0x100, 0x100);
            wram = bus.GetReferenceRange(0x200, 0x600);

            res = bus.GetReference(0xFFFC);
            nmi = bus.GetReference(0xFFFA);
            irq = brk = bus.GetReference(0xFFFE);

            registers = new Registers();
            stack = bus.GetReferenceRange(0x100, 0x100);

            core = new Core(registers);

            // -------------
            oldCpu = new OldCPU(core, registers, stack);
            // -------------
        }

        public int Step() {
            return oldCpu.Step();
        }

        public void ColdBoot() {
            oldCpu.ColdBoot();
        }

        public void WarmBoot() {
            oldCpu.WarmBoot();
        }

    }
}