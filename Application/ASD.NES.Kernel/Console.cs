using System;

namespace ASD.NES.Kernel {

    using ConsoleComponents;
    using Shared;

    public sealed partial class Console {

        private Clock Clk { get; set; }
        private CentralProcessor Cpu { get; set; }
        private PictureProcessor Ppu { get; set; }
        private AudioProcessor Apu { get; set; }

        public event Action<uint[]> NextFrameReady;
        public event Action PlayAudio {
            add => Apu.PlayAudio += value;
            remove => Apu.PlayAudio -= value;
        }

        public IAudioBuffer AudioBuffer => Apu.Buffer;
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

            if (Apu == null) { // ? TODO: impl. reset for pAPU
                Apu = new AudioProcessor();
            }

        }

        public uint[] Update() {

            var startingFrame = Ppu.TotalFrames;

            while (startingFrame == Ppu.TotalFrames) {

                var cycles = Cpu.Step();
                PpuStep(cycles);
                ApuStep(cycles);
            }
            return Ppu.ActualFrame;
        }

        public void PpuStep(int cpuCycles) {
            for (var i = 0; i < cpuCycles * 3; ++i) {
                Ppu.Step();
            }
        }

        int extraCpuCycle = 0;
        public void ApuStep(int cpuCycles) {
            cpuCycles += extraCpuCycle;
            for (var i = 0; i < cpuCycles / 2; i++) {
                Apu.Step();
            }
            extraCpuCycle = cpuCycles & 0x1;
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