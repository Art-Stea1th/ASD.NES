namespace ASD.NES.Kernel.ConsoleComponents.APUParts.Registers {

    // TODO: Separate channels & registers, Add 'base-classes' for them

    using BasicComponents;
    using Helpers;

    // http://wiki.nesdev.com/w/index.php/APU#Specification Noise ($400C-400F)
    // http://wiki.nesdev.com/w/index.php/APU_Noise
    internal sealed class NoiseChannel : IMemory<byte> {

        // ------- Registers -------

        private byte[] r = new byte[4];
        public byte this[int address] {
            get => r[address & 3];
            set => r[address & 3] = value;
        }
        public int Cells => r.Length;

        // register[0] - $400C : --LC VVVV : length counter halt (L), constant volume (C), volume/envelope (V)
        // !! WRITE for all or envelope only?
        public bool LengthCounterHalt => r[0].HasBit(5);
        public bool ConstantVolume => r[0].HasBit(4);
        public byte EnvelopeDividerPeriodOrVolume => r[0].L();

        // register[1] - $400D : ---- ---- : isn't used?

        // register[2] - $400E : M--- PPPP : !! WRITE ? Mode flag (M), Timer period ndx from table
        public bool ModeFlagIsSet => r[2].HasBit(7);
        public byte Period => r[2].L();

        // register[3] - $400F : LLLL L--- : !! WRITE ? Length counter load and envelope restart
        public byte LengthCounterLoad => (byte)(r[3] >> 3);



        // http://wiki.nesdev.com/w/index.php/APU_Noise // NTSC
        private int [] noiseFrequencyTable = {
            4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068
        };


        /*
         * noise chanell pseudo random
         * 
         * fb = shiftReg[0] ^ (shiftReg[6] if mode else shiftReg[1])
         * shiftReg >>= 1
         * shiftReg[14] = earler fb
         *
         * =>
         *
         * (93 or 31) if mode else 32767 step
         * 
         */

        int feedback = 0;
        int shiftReg = 1;
        private void Tmp() {

            if (ModeFlagIsSet) {
                feedback = (shiftReg >> 6 & 0b1) ^ (shiftReg & 0b1);
            }
            else {
                feedback = (shiftReg >> 1 & 0b1) ^ (shiftReg & 0b1);
            }

            shiftReg >>= 1;
            shiftReg = (shiftReg & 0x3FFF) | ((feedback & 1) << 14);
        }
    }
}