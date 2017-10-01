using System;

namespace ASD.NESCore {

    using ConsoleParts;

    public enum State { Off = 0x00, On = 0x01, Paused = 0x10, Busy = 0x11 }

    public sealed class Console {

        private CentralProcessor cpu;
        private PixelProcessor ppu;
        private Cartridge cartridge;

        public State State { get; private set; }
        public event Action<State> StateChanged;

        public event Func<uint[]> NextFrameReady;

        public Console() {

        }

        public Console(Cartridge cartridge) {
            this.cartridge = cartridge;
        }

        public void PowerOn() {
            if (State == State.Off) {

                State = State.On;
            }
        }

        public void PowerOff() {
            if (State > 0) {

                State = State.Off;
            }
        }

        public void Pause() {
            if (State == State.On) {

                State = State.Paused;
            }
        }

        public void Resume() {
            if (State == State.Paused) {

                State = State.On;
            }
        }

        public void Reset() {
            if (State == State.On || State == State.Paused) {
                PowerOff();
                PowerOn();
            }
            State = State.On;
        }
    }    
}