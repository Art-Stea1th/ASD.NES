namespace ASD.NES.Core.ConsoleComponents.CPUParts.Registers {

    using Shared;

    internal struct StateFlag {

        private readonly RefOctet register;
        private readonly int registerBit;

        public StateFlag(RefOctet register, int registerBit) {
            this.register = register;
            this.registerBit = registerBit;
        }

        public void Set(int value) => register[registerBit] = value != 0;
        public void Set(bool value) => register[registerBit] = value;

        public static implicit operator bool(StateFlag state) => state.register[state.registerBit];
        public static implicit operator byte(StateFlag state) => (byte)((state.register.Value >> state.registerBit) & 1);
    }
}
