namespace ASD.NES.Core.ConsoleComponents {

    using Shared;

    internal enum PlayerNumber { One, Two }

    internal sealed class InputHandler {

        private Octet shiftRegister;
        private InputState controllerOneInputState;
        private InputState controllerTwoInputState;

        public void ConnectController(IGamepad controller, PlayerNumber player) {
            switch (player) {
                case PlayerNumber.One: controllerOneInputState = new InputState(controller); break;
                case PlayerNumber.Two: controllerTwoInputState = new InputState(controller); break;
            }            
        }

        public void Write(int address, Octet value) {
            if (address == 0x4016) {
                var prev = shiftRegister;
                shiftRegister = value;

                if (prev[0] && !shiftRegister[0]) {
                    controllerOneInputState?.Reload();
                    controllerTwoInputState?.Reload();
                }
            }
        }

        public byte Read(int address) {
            switch (address) {
                case 0x4016: return controllerOneInputState != null ? controllerOneInputState.Next() : (byte)0;
                case 0x4017: return controllerTwoInputState != null ? controllerTwoInputState.Next() : (byte)0;
                default: return 0;
            }
        }

        private class InputState {

            private IGamepad controller;
            private Octet states;
            private byte keyBit;

            public InputState(IGamepad controller)
                => this.controller = controller;

            public byte Next()
                => keyBit < 8
                ? (byte)(states[keyBit++] ? 1 : 0)
                : (byte)0;

            public void Reload() {

                keyBit = states = 0;

                states[0] = controller.IsKeyDown(GamepadKey.A);
                states[1] = controller.IsKeyDown(GamepadKey.B);
                states[2] = controller.IsKeyDown(GamepadKey.Select);
                states[3] = controller.IsKeyDown(GamepadKey.Start);

                states[4] = controller.IsKeyDown(GamepadKey.Up);
                states[5] = controller.IsKeyDown(GamepadKey.Down);
                states[6] = controller.IsKeyDown(GamepadKey.Left);
                states[7] = controller.IsKeyDown(GamepadKey.Right);
            }
        }
    }
}