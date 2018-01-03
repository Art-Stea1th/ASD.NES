namespace ASD.NES.Kernel.ConsoleComponents {

    using CPUParts;
    using PPUParts;

    internal sealed class PictureProcessor {

        private CPUAddressSpace cpuMemory = CPUAddressSpace.Instance;
        private RegistersPPU r;
        private PPUCore core;
        private Scan scan;

        internal uint[] ActualFrame => core.Frame;
        internal int TotalFrames => scan.Frame;

        public PictureProcessor() {
            r = cpuMemory.RegistersPPU;
            core = new PPUCore(r);
            scan = new Scan();
        }

        public void Step() {

            // Y line  - [-1 - 260] start\end inclusive; -1 == 261
            // X point - [ 0 - 340] start\end inclusive;  0 == 341

            if (scan.Line == -1 && scan.Point == 1) {
                r.PpuStat.VBlank = false;
                r.PpuStat.SpriteZeroHit = false;
            }

            if (scan.Line >= 0 && scan.Line <= 239) {           //   0-239; [-1 - Pre-Render]
                if (scan.Point >= 1 && scan.Point <= 256) {     //   1-256; [ 0 - Idle      ]
                    core.RenderStep(scan.Point - 1, scan.Line);
                }
                if (scan.Point >= 257 && scan.Point <= 320) {   // 257-320;
                    core.ComputeSpriteForScanline(scan.Point - 257, scan.Line + 1);
                }
            }

            if (scan.Line == 241 && scan.Point == 1) {
                r.PpuStat.VBlank = true;
                if (r.PpuCtrl.NMIAtVBI) {
                    cpuMemory.Nmi = true;
                }
            }
            scan++;
        }

        public void WarmBoot() => ColdBoot();

        public void ColdBoot() {

            r.PpuCtrl.Clear();
            r.PpuMask.Clear();
            r.PpuStat.VBlank = false;

            r.PpuScrl.Value = 0;
            r.PpuAddr.Value = 0;
            r.PpuData.Value = 0;

            scan.Reset();
        }

        private class Scan {

            private int x, y, frame;
            private bool Even => (frame & 1) == 0;

            public int Point => x;
            public int Line => y;
            public int Frame => frame;

            public static Scan operator ++(Scan scan) {

                int lastX = scan.y == -1 && scan.Even ? 340 : 339, lastY = 260;

                scan.x++;
                if (scan.x > lastX) {
                    scan.x = 0; scan.y++;
                    if (scan.y > lastY) {
                        scan.y = -1; scan.frame++;
                    }
                }
                return scan;
            }
            public void Reset() { x = 0; y = 0; frame = 0; }
        }
    }
}