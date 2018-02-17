namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Registers {
    using System;

    // TODO: Separate channels & registers, Add 'base-classes' for them

    using BasicComponents;
    using Helpers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification Delta Modulation ($4010-4013)
    // http://wiki.nesdev.com/w/index.php/APU_DMC
    internal sealed class DeltaModulationChannel : IMemory<byte> {

        // ------- Registers -------

        private byte[] r = new byte[4];
        public byte this[int address] {
            get => r[address & 0b11];
            set => r[address & 0b11] = value;
        }
        public int Cells => r.Length;

        // register[0] - $4010 : IL-- RRRR : IRQ enabled (I), Loop (L), Rate index (R)
        public bool IRQEnabled => r[0].HasBit(7);
        public bool LoopMode => r[0].HasBit(6);
        public byte RateIndex => r[0].L();

        // register[1] - $4011 : -DDD DDDD : Direct load (D)
        public byte DirectLoad => (byte)(r[1] & 0x7F);

        // register[2] - $4012 : AAAA AAAA : sample Address (A)
        public byte SampleAddress => r[2];

        // register[3] - $4013 : LLLL LLLL : sample Length (L)
        public byte SampleLength => r[3];

        // https://wiki.nesdev.com/w/index.php/APU_DMC // NTSC
        private int[] RateTable { get; } = {
            428, 380, 340, 320, 286, 254, 226, 214, 190, 160, 142, 128, 106,  84,  72,  54
        };

        public float GetAudio(int timeInSamples, int sampleRate) {
            return 0f;
        }
    }
}