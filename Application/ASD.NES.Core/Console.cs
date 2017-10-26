﻿using System;

namespace ASD.NES.Core {

    using ConsoleComponents;
    using Shared;

    public sealed partial class Console {

        private Clock clock;

        internal CentralProcessor Cpu { get; private set; }
        internal PictureProcessor Ppu { get; private set; }

        public event Action<uint[]> NextFrameReady;

        public IGamepad PlayerOneController { set => Cpu.AddressSpace.InputPort.ConnectController(value, PlayerNumber.One); }
        public IGamepad PlayerTwoController { set => Cpu.AddressSpace.InputPort.ConnectController(value, PlayerNumber.Two); }

        public Console() {
            clock = new Clock(
                TimeSpan.FromMilliseconds(1000.0 / 60.0988) /*TimeSpan.FromTicks(4)*/,
                () => NextFrameReady?.Invoke(Update()));
            State = State.Off;
            InitializeHardware();
        }

        public void InsertCartridge(Cartridge cartridge) {
            ColdBoot();
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