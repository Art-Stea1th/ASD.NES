namespace ASD.NES.Kernel.ConsoleComponents {

    using BasicComponents;
    using Helpers;

    internal enum PlayerNumber { One, Two }

    internal sealed class InputPort : IMemory<byte> {

        private byte shiftRegister;

        public byte this[int address] {
            get => Read(address);
            set => Write(address, value);
        }
        public int Cells => 2;

        private InputState controllerOneInputState;
        private InputState controllerTwoInputState;

        public void ConnectController(IGamepad controller, PlayerNumber player) {
            switch (player) {
                case PlayerNumber.One: controllerOneInputState = new InputState(controller); break;
                case PlayerNumber.Two: controllerTwoInputState = new InputState(controller); break;
            }            
        }

        public byte Read(int address) {
            switch (address) {
                case 0x4016: return controllerOneInputState != null ? controllerOneInputState.Next() : (byte)0;
                case 0x4017: return controllerTwoInputState != null ? controllerTwoInputState.Next() : (byte)0;
                default: return 0;
            }
        }

        public void Write(int address, byte value) {
            if (address == 0x4016) {
                var prev = shiftRegister;
                shiftRegister = value;

                if (prev.HasBit(0) && !shiftRegister.HasBit(0)) {
                    controllerOneInputState?.Reload();
                    controllerTwoInputState?.Reload();
                }
            }
        }

        private class InputState {

            private IGamepad controller;
            private byte states;
            private byte keyBit;

            public InputState(IGamepad controller)
                => this.controller = controller;

            public byte Next()
                => keyBit < 8
                ? (byte)(states.HasBit(keyBit++) ? 1 : 0)
                : (byte)0;

            public void Reload() {

                keyBit = states = 0;

                states = BitOperations.MakeInt8(
                    controller.IsKeyDown(GamepadKey.Right),
                    controller.IsKeyDown(GamepadKey.Left),
                    controller.IsKeyDown(GamepadKey.Down),
                    controller.IsKeyDown(GamepadKey.Up),

                    controller.IsKeyDown(GamepadKey.Start),
                    controller.IsKeyDown(GamepadKey.Select),
                    controller.IsKeyDown(GamepadKey.B),
                    controller.IsKeyDown(GamepadKey.A)
                    );
            }
        }
    }
}