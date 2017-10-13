using System;
using System.Collections.Generic;
using System.Text;

namespace ASD.NES.Core.CommonComponents {

    internal sealed class PPUMemory : Memory {

    }

    internal sealed class CPUMemory : Memory {

        public byte[] ZeroPage { get; }
        public byte[] Stack { get; }
        public byte[] WRAM { get; }

        public byte[] PPURegisters { get; }

    }

    internal abstract class Memory {

    }
}