using System;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    /// <summary> "Zero page (Post-Indexed) Y" addressing mode </summary>
    internal sealed class ZPY_ : ZPG_ {
        public ZPY_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }

        protected override int GetAddress() {
            core.ClockTime();
            return base.GetAddress() + r.Y;
        }
    }

    /// <summary> "Zero page (Post-Indexed) X" addressing mode </summary>
    internal sealed class ZPX_ : ZPG_ {
        public ZPX_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }

        protected override int GetAddress() {
            core.ClockTime();
            return base.GetAddress() + r.X;
        }
    }

    /// <summary> "Zero page" addressing mode </summary>
    internal class ZPG_ : IMM_ {
        public ZPG_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }

        protected override int GetAddress() => Read(base.GetAddress());
    }

    /// <summary> "Immediate" addressing mode </summary>
    internal class IMM_ : AddressingModeNew {
        public IMM_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }

        protected override int GetAddress() => ++r.PC;      // bytes += 1

        protected override int Read(int address) {
            core.ClockTime();                               // cycles += 1
            return memory[address];
        }
    }

    /// <summary> Addressing mode </summary>
    internal abstract class AddressingModeNew {

        protected readonly CPUAddressSpace memory = CPUAddressSpace.Instance;
        protected readonly CPUCoreNew core;
        protected readonly RegistersCPU r;

        public int M {
            get => Read(GetAddress());
            set => Write(GetAddress(), value);
        }

        protected virtual int GetAddress() => throw new InvalidOperationException();
        protected virtual int Read(int address) => throw new InvalidOperationException();
        protected virtual void Write(int address, int value) => throw new InvalidOperationException();

        public AddressingModeNew(RegistersCPU registers, CPUCoreNew core) {
            r = registers; this.core = core;
        }
    }
}