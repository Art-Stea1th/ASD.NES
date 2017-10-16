namespace ASD.NES.Core {

    public enum State { Off = 0x00, On = 0x01, Paused = 0x10, Busy = 0x11 }

    public sealed partial class Console { // simple FSM

        public State State { get; private set; }

        public void PowerOn() {
            if (State == State.Off) {
                clock.Start();
                State = State.On;
            }
        }

        public void PowerOff() {
            if (State > 0) {
                clock.Stop();
                State = State.Off;
            }
        }

        public void Pause() {
            if (State == State.On) {
                clock.Stop();
                State = State.Paused;
            }
        }

        public void Resume() {
            if (State == State.Paused) {
                clock.Start();
                State = State.On;
            }
        }

        public void Reset() {

            if (State == State.On) {
                WarmBoot();
            }

            if (State == State.Off) {
                PowerOn();
                InitializeHardware();
                ColdBoot();
            }
            else if (State == State.Paused) {
                try {
                    Resume();
                    WarmBoot();
                }
                catch {
                    PowerOn();
                    InitializeHardware();
                    ColdBoot();
                }
            }
        }
    }
}