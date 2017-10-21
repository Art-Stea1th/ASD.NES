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
        private RegistersPPU r = CPUAddressSpace.Instance.RegistersPPU;

        private Scan scan = new Scan();

        public void Step() {
            oldPpu.Step();
        }

        private void RenderStep(int x, int y) {

        }

        public void WarmBoot() => ColdBoot();

        public void ColdBoot() {
            oldPpu.ColdBoot();
        }

        private struct Scan {

            private int x, y, frame;

            public int Point => x;
            public int Line => y;
            public int Frame => frame;

            public static Scan operator ++(Scan scan) {

                var even = (scan.frame & 1) == 0;
                int lastX = scan.y == -1 && even ? 341 : 340, lastY = 261;

                scan.x++;
                if (scan.x == lastX) {
                    scan.x = 0; scan.y++;
                    if (scan.y == lastY) {
                        scan.y = -1; scan.frame++;
                    }
                }
                return scan;
            }
            public void Reset() { x = 0; y = -1; frame = 0; }
        }
    }
}