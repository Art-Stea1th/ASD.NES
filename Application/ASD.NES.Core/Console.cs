using System;
using OldCode;

namespace ASD.NES.Core {

    using ConsoleComponents;
    using Shared;

    public enum State { Off = 0x00, On = 0x01, Paused = 0x10, Busy = 0x11 }

    public sealed class Console {

        internal CentralProcessor Cpu { get; private set; }
        internal PixelProcessor Ppu { get; private set; }
        internal OldIO Io { get; private set; }

        private IController controller;

        private Timer process;

        public event Action<uint[]> NextFrameReady;

        public State State { get; private set; }        

        public Console(IController controller) {
            process = new Timer(
                TimeSpan.FromMilliseconds(1000.0 / 128.863632),
                () => NextFrameReady?.Invoke(Update()));
            State = State.Off;
            OldMemoryBus.Instance.Console = this;
            this.controller = controller;
            Initialize();
            ColdBoot();
        }

        private void SendEmptyFrame() {
            NextFrameReady?.Invoke(new uint[256 * 240]);
        }

        public void InsertCartridge(Cartridge cartridge) { }

        public uint[] Update() {

            var startingFrame = Ppu.FrameCount;

            while (startingFrame == Ppu.FrameCount) {

                var cycles = Cpu.Step();
                for (var i = 0; i < cycles * 3; ++i) {
                    Ppu.Step();
                }
            }
            return Ppu.ImageData;
        }

        public void PowerOn() {
            if (State == State.Off) {
                process.Start();
                State = State.On;
            }
        }

        public void PowerOff() {
            if (State > 0) {
                process.Stop();
                State = State.Off;
            }
        }

        public void Pause() {
            if (State == State.On) {
                process.Stop();
                State = State.Paused;
            }
        }

        public void Resume() {
            if (State == State.Paused) {
                process.Start();
                State = State.On;
            }
        }

        public void Reset() {

            if (State == State.On) {
                WarmBoot();
            }

            if (State == State.Off) {
                PowerOn();
                Initialize();
                ColdBoot();
            }
            else if (State == State.Paused) {
                try {
                    Resume();
                    WarmBoot();
                }
                catch {
                    PowerOn();
                    Initialize();
                    ColdBoot();
                }
            }
        }

        private void Initialize() {
            Cpu = new CentralProcessor();
            Ppu = new PixelProcessor();
            Io = new OldIO(controller);            
        }

        private void ColdBoot() {
            Cpu.ColdBoot();
            Ppu.ColdBoot();
        }
        private void WarmBoot() {
            Cpu.WarmBoot();
            Ppu.WarmBoot();
        }
    }
}