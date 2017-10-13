using System;
using System.Collections.Generic;
using System.Text;

namespace ASD.NES.Core.CommonComponents {

    using Shared;

    internal sealed class MemoryBus {

        private RInt8 cpuRam;
        private RInt8 ppuRam;
        
        //public RInt8 GetReference(int address)
        //    => memory[address];

        //public RInt8[] GetReferenceRange(int startAddress, int count)
        //    => memory.Skip(startAddress).Take(count).ToArray();

    }
}