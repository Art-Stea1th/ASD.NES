namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    using Shared;

    internal struct StateFlag {

        private readonly RInt8 register;
        private readonly int registerBit;

        public StateFlag(RInt8 register, int registerBit) {
            this.register = register;
            this.registerBit = registerBit;
        }

        public void Set(int value) => register[registerBit] = value != 0;
        public void Set(bool value) => register[registerBit] = value;

        public static implicit operator bool(StateFlag state) => state.register[state.registerBit];
        public static implicit operator byte(StateFlag state) => (byte)((state.register >> state.registerBit) & 1);
    }
}
