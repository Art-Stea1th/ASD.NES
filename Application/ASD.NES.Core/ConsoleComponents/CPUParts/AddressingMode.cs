using System;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    using Shared;

    /// <summary> "Indirect Y" (Post-Indexed Indirect) addressing mode </summary>
    internal sealed class IDY_ : IMM_ {
        public IDY_(RegistersCPU r, CPUCore c) : base(r, c) { }
    }

    /// <summary> "Indirect X" (Pre-Indexed Indirect) addressing mode </summary>
    internal sealed class IDX_ : IMM_ {
        public IDX_(RegistersCPU r, CPUCore c) : base(r, c) { }
    }

    /// <summary> "Indirect" addressing mode </summary>
    internal class IND_ : IMM_ {
        public IND_(RegistersCPU r, CPUCore c) : base(r, c) { }
    }

    /// <summary> "Absolute (Post-Indexed) Y" addressing mode </summary>
    internal sealed class ABY_ : ABS_ {
        public ABY_(RegistersCPU r, CPUCore c) : base(r, c) { }

        public override ushort ReadAddress() {
            return (ushort)(base.ReadAddress() + Y());
        }
    }

    /// <summary> "Absolute (Post-Indexed) X" addressing mode </summary>
    internal sealed class ABX_ : ABS_ {                              // r: 3,4*(5) w: 3,7
        public ABX_(RegistersCPU r, CPUCore c) : base(r, c) { }

        public override ushort ReadAddress() {
            return (ushort)(base.ReadAddress() + X());
        }
    }

    /// <summary> "Absolute" addressing mode </summary>
    internal class ABS_ : IMM_ {                                     // r: 3,4 w: 3,6
        public ABS_(RegistersCPU r, CPUCore c) : base(r, c) { }

        public override ushort ReadAddress() {

            AddrOne = ReadMemory(base.ReadAddress());                // 1,1 ok
            AddrTwo = ReadMemory(base.ReadAddress());                // 1,1 ok

            return (ushort)(((AddrTwo & 0xFF) << 8) | (AddrOne & 0xFF));
        }
    }

    /// <summary> "Zero page (Post-Indexed) Y" addressing mode </summary>
    internal sealed class ZPY_ : ZPG_ {                              // r: 2,4 w: 2,6
        public ZPY_(RegistersCPU r, CPUCore c) : base(r, c) { }

        public override ushort ReadAddress() {                       // 1,2 ok
            return (ushort)(base.ReadAddress() + Y());
        }
    }

    /// <summary> "Zero page (Post-Indexed) X" addressing mode </summary>
    internal sealed class ZPX_ : ZPG_ {                              // r: 2,4 w: 2,6
        public ZPX_(RegistersCPU r, CPUCore c) : base(r, c) { }

        public override ushort ReadAddress() {                       // 1,2 ok
            return (ushort)(base.ReadAddress() + X());
        }
    }

    /// <summary> "Zero page" addressing mode </summary>
    internal class ZPG_ : IMM_ {                                     // r: 2,3 w: 2,5
        public ZPG_(RegistersCPU r, CPUCore c) : base(r, c) { }

        public override ushort ReadAddress() {                       // 1,1 ok
            return AddrOne = ReadMemory(base.ReadAddress());
        }
    }

    /// <summary> "Immediate" addressing mode </summary>
    internal class IMM_ : AddressingMode {                        // r: 2,2 w: -,-
        public IMM_(RegistersCPU r, CPUCore c) : base(r, c) { }

        public override ushort ReadAddress() {                       // 1,0 ok
            return Argument;
        }
        public override int Read() {                                 // 0,1 ok
            return ReadMemory(AddrOne = ReadAddress());
        }
        public override void Write(int value) {                      // 0,2 - inheritance only
            Clock(); WritMemory(AddrOne, value);
        }
        public override void WriteOnly(int value) {                  // 0,1 - inheritance only
            WritMemory(ReadAddress(), value);
        }
    }

    /// <summary> Addressing mode </summary>
    internal abstract class AddressingMode { // in core - r/w: 1,1 (before: c++, after: b++)

        private readonly CPUAddressSpace memory = CPUAddressSpace.Instance;
        private readonly CPUCore core;
        private readonly RegistersCPU r;

        protected ushort AddrOne, AddrTwo;

        public virtual ushort ReadAddress() => throw new InvalidOperationException();
        public virtual int Read() => throw new InvalidOperationException();
        public virtual void Write(int value) => throw new InvalidOperationException();
        public virtual void WriteOnly(int value) => throw new InvalidOperationException();

        protected virtual ushort Argument => ++r.PC;                 // b++

        protected virtual byte ReadMemory(int address) {             // c++
            core.ClockTime(); return memory[address];
        }
        protected virtual byte ReadMemoryX(int address) {            // ??
            return memory[address];
        }
        protected virtual byte X() {                                 // c++
            core.ClockTime(); return r.X;
        }
        protected virtual byte Y() {                                 // c++
            core.ClockTime(); return r.Y;
        }
        protected virtual void WritMemory(int address, int value) {  // c++
            core.ClockTime(); memory[address] = (byte)value;
        }
        protected void Clock() => core.ClockTime();

        public bool SamePage(ushort addressA, ushort addressB)
            => (addressA & 0xFF00) == (addressB & 0xFF00);

        public AddressingMode(RegistersCPU registers, CPUCore core) {
            r = registers; this.core = core;
        }
    }
}