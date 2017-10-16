using System.Collections.Generic;
using OldCode;

namespace ASD.NES.Core.ConsoleComponents {

    using PPUParts;

    internal sealed class PictureProcessor {

        // -------------
        private OldPPU oldPpu;
        // -------------

        private RegistersPPU r;

        public long FrameCount => oldPpu.FrameCount;
        public uint[] ImageData => oldPpu.ImageData;


        public PictureProcessor() {
            r = new RegistersPPU();
            oldPpu = new OldPPU(r);
        }

        public void Write(int addr, byte val) {
            oldPpu.Write(addr, val);
        }

        public byte Read(ushort addr) {
            return oldPpu.Read(addr);
        }

        public void Step() {
            oldPpu.Step();
        }

        public void ColdBoot() {
            oldPpu.ColdBoot();
        }

        public void WarmBoot() {
            oldPpu.WarmBoot();
        }
    }
}