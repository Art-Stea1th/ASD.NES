using System;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    using Shared;

    /// <summary> "Indirect Y" (Post-Indexed Indirect) addressing mode </summary>
    internal sealed class IDY_ : IMM_ {
        public IDY_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }
    }

    /// <summary> "Indirect X" (Pre-Indexed Indirect) addressing mode </summary>
    internal sealed class IDX_ : IMM_ {
        public IDX_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }
    }

    /// <summary> "Indirect" addressing mode </summary>
    internal class IND_ : IMM_ {
        public IND_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }
    }

    /// <summary> "Absolute (Post-Indexed) Y" addressing mode </summary>
    internal sealed class ABY_ : ABS_ {
        public ABY_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }
    }

    /// <summary> "Absolute (Post-Indexed) X" addressing mode </summary>
    internal sealed class ABX_ : ABS_ {
        public ABX_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }
    }

    /// <summary> "Absolute" addressing mode </summary>
    internal class ABS_ : IMM_ {
        public ABS_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }
    }

    /// <summary> "Zero page (Post-Indexed) Y" addressing mode </summary>
    internal sealed class ZPY_ : ZPG_ {
        public ZPY_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }
    }

    /// <summary> "Zero page (Post-Indexed) X" addressing mode </summary>
    internal sealed class ZPX_ : ZPG_ {
        public ZPX_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }
    }

    /// <summary> "Zero page" addressing mode </summary>
    internal class ZPG_ : IMM_ {                                  // r: 2,3 w: 2,5
        public ZPG_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }
        public override int M {
            get { // 1,2 ok
                ReadAdr(); ReadMem(); Adr = Mem; ReadMem(); return Mem; 
            }
            set { // 0,2 ok
                SetMem(value); WritMem();                               
            }
        }
    }

    /// <summary> "Immediate" addressing mode </summary>
    internal class IMM_ : AddressingModeNew {                     // r: 2,2 w: -,-
        public IMM_(RegistersCPU r, CPUCoreNew c) : base(r, c) { }
        public override int M {
            get { // 1,1 ok
                ReadAdr(); ReadMem(); return Mem;
            }
        }
    }

    /// <summary> Addressing mode </summary>
    internal abstract class AddressingModeNew { // in core - r/w: 1,1 (before: c++, after: b++)

        private readonly CPUAddressSpace memory = CPUAddressSpace.Instance;
        private readonly CPUCoreNew core;
        private readonly RegistersCPU r;

        protected int Adr, Mem, RgX, RgY;
        public virtual int M { get; set; }

        // Access
        protected virtual void ReadAdr() { // b++
            r.PC++;
            Adr = r.PC;
        }
        protected virtual void ReadMem() { // c++
            core.ClockTime();
            Mem = memory[Adr];
        }
        protected virtual void ReadRgX() { // c++
            core.ClockTime();
            RgX = r.X;
        }
        protected virtual void ReadRgY() { // c++
            core.ClockTime();
            RgY = r.Y;
        }

        // Mutate
        protected virtual void SetMem(int value) { // c++
            core.ClockTime();
            Mem = value;
        }
        protected virtual void WritMem() { // c++
            core.ClockTime();
            memory[Adr] = (byte)Mem;
        }


        public bool SamePage(ushort addressA, ushort addressB)
            => (addressA & 0xFF00) == (addressB & 0xFF00);

        public AddressingModeNew(RegistersCPU registers, CPUCoreNew core) {
            r = registers; this.core = core;
        }
    }
}