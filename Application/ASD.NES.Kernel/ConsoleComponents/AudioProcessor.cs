using System;
using System.Collections.Generic;
using System.Text;

namespace ASD.NES.Kernel.ConsoleComponents {

    using APUParts;
    using CPUParts;

    internal sealed class AudioProcessor {

        private CPUAddressSpace cpuMemory = CPUAddressSpace.Instance;
        private RegistersAPU r;

        public AudioProcessor() {
            r = cpuMemory.RegistersAPU;
        }

    }
}