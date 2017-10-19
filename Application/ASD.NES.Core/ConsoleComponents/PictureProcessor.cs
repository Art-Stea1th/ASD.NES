using OldCode;

namespace ASD.NES.Core.ConsoleComponents {

    using CPUParts;
    using PPUParts;

    internal sealed class PictureProcessor {

        #region ------------- Old Code -------------
        private OldPPU oldPpu = new OldPPU();
        public long OldFrameCount => oldPpu.FrameCount;
        public uint[] OldImageData => oldPpu.ImageData;
        #endregion ---------- Old Code -------------

        private CPUAddressSpace cpuMemory = CPUAddressSpace.Instance;
        private PPUAddressSpace ppuMemory = PPUAddressSpace.Instance;
        private RegistersPPU r;

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