using System;
    using System.Collections.Generic;

namespace ASD.NESCore {

    using Common;
    using ConsoleParts;

    public enum State { Off = 0x00, On = 0x01, Paused = 0x10, Busy = 0x11 }

    public sealed class Console {

        private CentralProcessor cpu;
        private PixelProcessor ppu;
        private MemoryBus memory;

        internal IReadOnlyCollection<RInt8> CpuRam { get; private set; }
        internal IReadOnlyCollection<RInt8> PpuRam { get; private set; }
        internal IReadOnlyCollection<RInt8> ApuRam { get; private set; }

        private Cartridge cartridge;

        public State State { get; private set; }
        public event Action<State> StateChanged;

        public event Func<uint[]> NextFrameReady;

        public Console() {
            CpuRam = new MemoryBank(2048, 4);
            PpuRam = new MemoryBank(8, 1024);
            ApuRam = new MemoryBank(4096, 1);
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