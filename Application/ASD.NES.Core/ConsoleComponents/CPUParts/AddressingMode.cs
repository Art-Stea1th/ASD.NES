using OldCode;

namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    using Helpers;

    /// <summary> "Absolute Y" addressing mode</summary>
    internal sealed class ABY : ABS {
        protected override int Address => base.Address + r.Y;
        public override int M => bus.Read(Address);
    }

    /// <summary> "Absolute X" addressing mode</summary>
    internal sealed class ABX : ABS {
        protected override int Address => base.Address + r.X;
        public override int M => bus.Read(Address);
    }

    /// <summary> "Absolute" addressing mode</summary>
    internal class ABS : AddressingMode {
        protected override int Address => BitOperations.MakeInt16(bus.Read(r.PC + 1), bus.Read(r.PC + 2));
        public override int M => bus.Read(Address);
    }

    /// <summary> "Zero page Y" addressing mode</summary>
    internal sealed class ZPY : ZPG {
        protected override int Address => base.Address + r.Y;
        public override int M => bus.Read(Address);
    }

    /// <summary> "Zero page X" addressing mode</summary>
    internal sealed class ZPX : ZPG {
        protected override int Address => base.Address + r.X;
        public override int M => bus.Read(Address);
    }

    /// <summary> "Zero page" addressing mode</summary>
    internal class ZPG : AddressingMode {
        protected override int Address => bus.Read(r.PC + 1);
        public override int M => bus.Read(Address);
    }

    /// <summary> "Immediate" addressing mode</summary>
    internal sealed class IMM : AddressingMode {
        protected override int Address => r.PC + 1;
        public override int M => bus.Read(Address);
    }

    internal abstract class AddressingMode {

        protected OldMemoryBus bus = OldMemoryBus.Instance;
        protected Registers r;

        protected virtual int Address => 0;
        public virtual int M => 0;
        public virtual bool PageCrossed => false;

        //public byte Immediate(byte argOne) {
        //    return argOne;
        //}
        //public byte ZeroPage(ushort argOne) {
        //    return bus.Read(argOne);
        //}
        //public byte ZeroPageX(ushort argOne) {
        //    return bus.Read(argOne + r.X);
        //}
        //public byte ZeroPageY(ushort argOne) {
        //    return bus.Read(argOne + r.Y);
        //}
        //public byte Absolute(byte argOne, byte argTwo) {
        //    return bus.Read(MakeAddress(argOne, argTwo));
        //}
        //public byte AbsoluteX(byte argOne, byte argTwo) {
        //    return bus.Read(MakeAddress(argOne, argTwo) + r.X);
        //}
        //public byte AbsoluteY(byte argOne, byte argTwo) {
        //    return bus.Read(MakeAddress(argOne, argTwo) + r.Y);
        //}
        public byte IndirectX(byte argOne) {
            return bus.Read(ReadX2(bus.Read(argOne + r.X)));
        }
        public byte IndirectY(byte argOne) {
            var a = r.PC + 1;
            var address = BitOperations.MakeInt16(bus.Read(a), bus.Read(a + 1));
            return bus.Read(ReadX2(argOne) + r.Y);
        }


        // ------------------------

        public ushort MakeAddress(byte argOne, byte argTwo)
            => (ushort)((argTwo << 8) & argOne);
        public bool SamePage(ushort addressOne, ushort addressTwo) {
            return (addressOne & 0xFF00) == (addressTwo & 0xFF00);
        }
        public ushort ReadX2(ushort address) {
            return (ushort)((bus.Read((ushort)(address + 1)) << 8) | bus.Read(address));
        }
    }
}