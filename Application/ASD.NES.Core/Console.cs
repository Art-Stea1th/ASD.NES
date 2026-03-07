using System;

namespace ASD.NES.Core {

    using ConsoleComponents;
    using ConsoleComponents.PPUParts;
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

        /// <summary> If set, overrides cartridge region (e.g. PAL for "(E)" dumps that lack the header flag). Null = use cartridge.Region. </summary>
        public TvRegion? PreferRegion { get; set; }

        public Console() {
            State = State.Off;
            InitializeHardware();
        }

        public void InsertCartridge(Cartridge cartridge) {
            var region = PreferRegion ?? cartridge.Region;
            var profile = TvRegionProfile.For(region);
            Clk.Interval = profile.FrameInterval;
            Ppu.SetRegion(region);
            Apu.SetRegion(region);
            Cpu.ClearRAM();
            Ppu.ClearVideoState();
            ColdBoot();
        }

        private void InitializeHardware() {

            // change to hfq clk (1000ms / 60 / 88660 ~ 0.0001879... ~ every second timespan tick) ??
            Clk = new Clock(TimeSpan.FromMilliseconds(1000.0 / 60.0988));
            Clk.Tick += () => NextFrameReady?.Invoke(Update());

            Cpu = new CentralProcessor();
            Ppu = new PictureProcessor();

            if (Apu == null) { // TODO: impl. reset for pAPU
                Apu = new AudioProcessor();
            }

        }

        public uint[] Update() {

            var startingFrame = Ppu.TotalFrames;

            while (startingFrame == Ppu.TotalFrames) {

                var cycles = Cpu.Step();
                PpuStep(cycles);
                ApuStep(cycles);
                // OAM DMA steals 513 CPU cycles (511 after the 2-cycle write); NESDEV
                if (RegistersPPU.OamDmaCyclePenaltyPending) {
                    RegistersPPU.OamDmaCyclePenaltyPending = false;
                    PpuStep(511);
                    ApuStep(511);
                }
            }
            // Apu.ResetStepCounter(); // here doesn't solve unsync. apu #8 issue
            return Ppu.ActualFrame;
        }

        public void PpuStep(int cpuCycles) {
            for (var i = 0; i < cpuCycles * 3; ++i) {
                Ppu.Step();
            }
        }

        public void ApuStep(int cpuCycles) {
            Apu.StepCpuCycles(cpuCycles);
        }

        private void ColdBoot() {
            Cpu.ColdBoot();
            Ppu.ColdBoot();
        }

        private void WarmBoot() {
            Cpu.WarmBoot();
            Ppu.WarmBoot();
        }

        /// <summary> For tests: run exactly n CPU steps (each step = one instruction + PPU/APU cycles). </summary>
        internal void RunCpuSteps(int n) {
            for (var i = 0; i < n; i++) {
                var cycles = Cpu.Step();
                PpuStep(cycles);
                ApuStep(cycles);
            }
        }

        /// <summary> For tests: current CPU state. </summary>
        internal CpuState GetCpuState() => Cpu.GetState();

        /// <summary> For tests: read one byte from CPU address space. </summary>
        internal byte GetMemory(ushort address) => Cpu.AddressSpace[address];

        /// <summary> For tests: write one byte to CPU address space (e.g. PPU/APU registers at 0x2000–0x4017). </summary>
        internal void SetMemory(ushort address, byte value) => Cpu.AddressSpace[address] = value;

        /// <summary> For tests: set program counter (e.g. nestest.nes start at $C000 per NESDEV). </summary>
        internal void SetPC(ushort pc) => Cpu.SetPC(pc);

        /// <summary> For tests: set PPU nametable mirroring without loading a cartridge. </summary>
        internal void SetPpuMirroring(Mirroring m) => PPUAddressSpace.Instance.NametableMirroring = m;

    }
}