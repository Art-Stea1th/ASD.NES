namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Channels {

    using BasicComponents;
    using Helpers;
    using Registers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification Delta Modulation ($4010-4013)
    // http://wiki.nesdev.com/w/index.php/APU_DMC
    internal sealed class DeltaModulationChannel : AudioChannel {

        // ------- Registers -------

        private DeltaModulationChannelRegisters r;
        public override IMemory<byte> Registers => r;

        // register[0] - $4010 : IL-- RRRR : IRQ enabled (I), Loop (L), Rate index (R)
        public bool IRQEnabled => r[0].HasBit(7);
        public bool LoopMode => r[0].HasBit(6);
        public byte RateIndex => r[0].L();

        // register[1] - $4011 : -DDD DDDD : Direct load (D)
        public byte DirectLoad { get => (byte)(r[1] & 0x7F); set => r[1] = (byte)(value & 0x7F); }

        // register[2] - $4012 : AAAA AAAA : sample Address (A)
        public byte SampleAddress { get => r[2]; set => r[2] = value; }

        // register[3] - $4013 : LLLL LLLL : sample Length (L)
        public byte SampleLength { get => r[3]; set => r[3] = value; }

        // https://wiki.nesdev.com/w/index.php/APU_DMC // NTSC
        private int[] RateTable { get; } = {
            428, 380, 340, 320, 286, 254, 226, 214, 190, 160, 142, 128, 106,  84,  72,  54
        };

        public DeltaModulationChannel(DeltaModulationChannelRegisters registers) {
            r = registers;
            r.Changed += OnRegisterChanged;
        }

        public float GetAudio(int timeInSamples, int sampleRate) {
            return 0f;
        }

        public override float GetAudio() {
            throw new System.NotImplementedException();
        }

        public override void OnRegisterChanged(int address) {
            switch (address & 0b11) {
                case 0b00:
                    break;
                case 0b01:
                    break;
                case 0b10:
                    break;
                case 0b11:
                    break;
            }
        }
    }
}