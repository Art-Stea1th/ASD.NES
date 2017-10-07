using System;
using System.Linq;
using OldCode;

namespace ASD.NES.Core {

    using Shared;
    using ConsoleComponents;

    public enum State { Off = 0x00, On = 0x01, Paused = 0x10, Busy = 0x11 }

    public sealed class Console {

        internal CentralProcessor Cpu { get; private set; }
        internal PixelProcessor Ppu { get; private set; }
        internal OldIO Io { get; private set; }

        private IController controller;

        private long cpuCycle;
        private Timer process;

        public event Action<uint[]> NextFrameReady;

        public State State { get; private set; }        

        public Console(IController controller) {
            process = new Timer(
                TimeSpan.FromMilliseconds(1000.0 / 100),
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

        public void InsertCartridge(Cartridge cartridge) {
            //Initialize();
            //PowerOn();
        }

        public uint[] Update() {

            var startingFrame = Ppu.FrameCount;
            var steps = 0;

            while (true) {
                Step();
                steps++;
                if (startingFrame != Ppu.FrameCount) {
                    break;
                }
            }
            return Ppu.ImageData.Select(p => p >> 8).ToArray();
        }

        public void Step() {
            var cycles = Cpu.Step();
            cpuCycle += cycles;
            for (var i = 0; i < cycles * 3; ++i) {
                Ppu.Step();
            }
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
            PowerOff();
            Initialize();
            ColdBoot();
            PowerOn();
        }

        private void Initialize() {
            Cpu = new CentralProcessor();
            Ppu = new PixelProcessor();
            Io = new OldIO(controller);            
        }

        private void ColdBoot() {
            cpuCycle = 0;
            Cpu.ColdBoot();
            Ppu.ColdBoot();
        }
        private void WarmBoot() {
            cpuCycle = 0;
            Cpu.WarmBoot();
            Ppu.WarmBoot();
        }
    }
}