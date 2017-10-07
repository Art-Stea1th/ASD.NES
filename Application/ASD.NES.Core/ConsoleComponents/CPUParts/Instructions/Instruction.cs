using System;

namespace ASD.NES.Core.ConsoleComponents.CPUParts.Instructions {

    using Shared;

    internal abstract class Instruction {

        protected readonly Registers r;
        protected readonly RInt8[] stack;

        private string Name => GetType().Name;

        public Instruction(Registers registers, RInt8[] stack) {
            r = registers; this.stack = stack;
        }

        /// <summary> Immediate addressing mode </summary>
        public virtual int IMM()
            => throw new InvalidInstructionException($"{Name}.{nameof(IMM)}");

        /// <summary> Absolute addressing mode </summary>
        public virtual int ABS()
            => throw new InvalidInstructionException($"{Name}.{nameof(ABS)}");

        /// <summary> Zero-page Absolute addressing mode </summary>
        public virtual int ZPG()
            => throw new InvalidInstructionException($"{Name}.{nameof(ZPG)}");

        /// <summary> Implied addressing mode </summary>
        public virtual int IMP()
            => throw new InvalidInstructionException($"{Name}.{nameof(IMP)}");

        /// <summary> Accumulator addressing mode </summary>
        public virtual int ACC()
            => throw new InvalidInstructionException($"{Name}.{nameof(ACC)}");

        /// <summary> Zero-page Indexed addressing mode </summary>
        public virtual int ZPX()
            => throw new InvalidInstructionException($"{Name}.{nameof(ZPX)}");

        /// <summary> Indexed X addressing mode </summary>
        public virtual int ABX()
            => throw new InvalidInstructionException($"{Name}.{nameof(ABX)}");

        /// <summary> Indexed Y addressing mode </summary>
        public virtual int ABY()
            => throw new InvalidInstructionException($"{Name}.{nameof(ABY)}");

        /// <summary> Indirect addressing mode </summary>
        public virtual int IND()
            => throw new InvalidInstructionException($"{Name}.{nameof(IND)}");

        /// <summary> Indirect Pre-indexed addressing mode </summary>
        public virtual int INX()
            => throw new InvalidInstructionException($"{Name}.{nameof(INX)}");

        /// <summary> Indirect Post-indexed addressing mode </summary>
        public virtual int INY()
            => throw new InvalidInstructionException($"{Name}.{nameof(INY)}");

        /// <summary> Relative addressing mode </summary>
        public virtual int REL()
            => throw new InvalidInstructionException($"{Name}.{nameof(REL)}");

        /// <summary> Invalid addressing mode </summary>
        public int ___()
            => throw new InvalidInstructionException($"{Name}.{nameof(___)}");

        private class InvalidInstructionException : InvalidOperationException {
            public InvalidInstructionException(string instructionName)
                : base(instructionName) { }
        }
    }
}