using OldCode;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    using Helpers;

    /// <summary> "Indirect Y" addressing mode</summary>
    internal sealed class INY : AddressingMode {
        protected override int Address => BitOperations.MakeInt16(bus.Read(ArgOne + 1), bus.Read(ArgOne)) + r.Y;
        public override byte M => bus.Read(Address);
        public override bool PageCrossed => (Address & 0x0000_FF00) != ((Address - r.Y) & 0x0000_FF00);
        public INY(Registers registers) : base(registers) { }
    }

    /// <summary> "Indirect X" addressing mode</summary>
    internal sealed class INX : AddressingMode {
        protected override int Address => BitOperations.MakeInt16(bus.Read(ArgOne + r.X + 1), bus.Read(ArgOne + r.X));
        public override byte M => bus.Read(Address);
        public INX(Registers registers) : base(registers) { }
    }

    /// <summary> "Absolute Y" addressing mode</summary>
    internal sealed class ABY : ABS {
        protected override int Address => base.Address + r.Y;
        public override bool PageCrossed => (Address & 0x0000_FF00) != (base.Address & 0x0000_FF00);
        public ABY(Registers registers) : base(registers) { }
    }

    /// <summary> "Absolute X" addressing mode</summary>
    internal sealed class ABX : ABS {
        protected override int Address => base.Address + r.X;
        public override bool PageCrossed => (Address & 0x0000_FF00) != (base.Address & 0x0000_FF00);
        public ABX(Registers registers) : base(registers) { }
    }

    /// <summary> "Absolute" addressing mode</summary>
    internal class ABS : AddressingMode {
        protected override int Address => BitOperations.MakeInt16(ArgOne, ArgTwo);
        public override byte M { get => bus.Read(Address); set => bus.Write(Address, value); }
        public ABS(Registers registers) : base(registers) { }
    }

    /// <summary> "Zero page Y" addressing mode</summary>
    internal sealed class ZPY : ZPG {
        protected override int Address => base.Address + r.Y;
        public ZPY(Registers registers) : base(registers) { }
    }

    /// <summary> "Zero page X" addressing mode</summary>
    internal sealed class ZPX : ZPG {
        protected override int Address => base.Address + r.X;
        public ZPX(Registers registers) : base(registers) { }
    }

    /// <summary> "Zero page" addressing mode</summary>
    internal class ZPG : AddressingMode {
        protected override int Address => ArgOne;
        public override byte M { get => bus.Read(Address); set => bus.Write(Address, value); }
        public ZPG(Registers registers) : base(registers) { }
    }    

    /// <summary> "Immediate" addressing mode</summary>
    internal class IMM : AddressingMode {
        public override byte M => ArgOne;
        public IMM(Registers registers) : base(registers) { }
    }

    /// <summary> "Accumulator" addressing mode</summary>
    internal class ACC : AddressingMode {
        public override byte M { get => r.A; set => r.A = value; }
        public ACC(Registers registers) : base(registers) { }
    }

    internal abstract class AddressingMode {

        protected readonly OldMemoryBus bus = OldMemoryBus.Instance;
        protected readonly Registers r;

        protected virtual byte ArgOne => bus.Read(r.PC + 1);
        protected virtual byte ArgTwo => bus.Read(r.PC + 2);

        protected virtual int Address => 0;
        public virtual bool PageCrossed => false;
        public virtual byte M { get; set; }

        public AddressingMode(Registers registers) {
            r = registers;
        }
    }
}