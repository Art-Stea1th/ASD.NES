using System;
using OldCode;

namespace ASD.NES.Core {

    using ConsoleComponents;
    using Shared;

    public sealed partial class Console {

        private Timer clock;

        private readonly Octet[] cpuRAM = new Octet[2048];
        private readonly Octet[] ppuRAM = new Octet[2048];

        internal CentralProcessor Cpu { get; private set; }
        internal PictureProcessor Ppu { get; private set; }
        internal InputHandler Input { get; private set; }

        public event Action<uint[]> NextFrameReady;

        public IGamepad PlayerOneController { set => Input.ConnectController(value, PlayerNumber.One); }
        public IGamepad PlayerTwoController { set => Input.ConnectController(value, PlayerNumber.Two); }

        public Console() {
            clock = new Timer(
                TimeSpan.FromMilliseconds(1000.0 / 21.477272 / 6),
                () => NextFrameReady?.Invoke(Update()));
            State = State.Off;
            OldMemoryBus.Instance.Console = this;
            InitializeHardware();
            Input = new InputHandler();
            ColdBoot();
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

        private void InitializeHardware() {
            Cpu = new CentralProcessor();
            Ppu = new PictureProcessor();
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