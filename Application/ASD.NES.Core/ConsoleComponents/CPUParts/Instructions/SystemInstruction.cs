using System;
using System.Collections.Generic;
using System.Text;

namespace ASD.NES.Core.ConsoleComponents.CPUParts.Instructions {

    using Shared;

    /// <summary> Force an interrupt </summary>
    internal sealed class BRK : SystemInstruction {
        public BRK(Registers registers, RInt8[] stack) : base(registers, stack) { }
        public override int IMP() {
            return base.IMP();
        }
    }

    internal sealed class NOP : SystemInstruction {
        public NOP(Registers registers, RInt8[] stack) : base(registers, stack) { }
    }

    internal sealed class RTI : SystemInstruction {
        public RTI(Registers registers, RInt8[] stack) : base(registers, stack) { }
    }

    internal abstract class SystemInstruction : Instruction {
        public SystemInstruction(Registers registers, RInt8[] stack) : base(registers, stack) { }
    }
}