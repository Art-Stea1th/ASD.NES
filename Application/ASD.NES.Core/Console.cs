using System;

namespace ASD.NES.Core {

    using ConsoleComponents;
    using Shared;

    public sealed partial class Console {

        private Clock Clk { get; set; }
        private CentralProcessor Cpu { get; set; }
        private PictureProcessor Ppu { get; set; }

        public event Action<uint[]> NextFrameReady;

        public IGamepad PlayerOneController { set => Cpu.AddressSpace.InputPort.ConnectController(value, PlayerNumber.One); }
        public IGamepad PlayerTwoController { set => Cpu.AddressSpace.InputPort.ConnectController(value, PlayerNumber.Two); }

        public Console() {
            State = State.Off;
            InitializeHardware();
        }

        public void InsertCartridge(Cartridge cartridge) {
            ColdBoot();
        }

        private void InitializeHardware() {

            Clk = new Clock(TimeSpan.FromMilliseconds(1000.0 / 60.0988) /*TimeSpan.FromTicks(4)*/);
            Clk.Tick += () => NextFrameReady?.Invoke(Update());

            Cpu = new CentralProcessor();
            Ppu = new PictureProcessor();

            // 3 PPU steps on CPU tick
            Cpu.Clock += () => {
                Ppu.Step();
                Ppu.Step();
                Ppu.Step();
            };
        }

        public uint[] Update() {

            var startingFrame = Ppu.TotalFrames;

            while (startingFrame == Ppu.TotalFrames) {

                var cycles = Cpu.Step();
                for (var i = 0; i < cycles * 3; ++i) {
                    Ppu.Step();
                }
            }
            return Ppu.ActualFrame;
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