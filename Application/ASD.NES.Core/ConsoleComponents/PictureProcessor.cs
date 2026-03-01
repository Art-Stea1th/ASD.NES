namespace ASD.NES.Core.ConsoleComponents {

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
            scan = new Scan(260); // NTSC default: 262 scanlines (0..260)
        }

        /// <summary> Set TV region: NTSC 262 scanlines, PAL 312 scanlines. VBlank at line 241 for both (2C07). </summary>
        internal void SetRegion(TvRegion region) {
            scan.SetLastScanline(region == TvRegion.PAL ? 311 : 260);
        }

        public void Step() {

            // Y line  - [-1 - lastScanline] (NTSC 260, PAL 311). X point - [ 0 - 340], 341 dots/scanline

            if (scan.Line == -1 && scan.Point == 1) {
                r.PpuStat.VBlank = false;
                r.PpuStat.SpriteZeroHit = false;
            }

            if (scan.Line >= 0 && scan.Line <= 239) {           //   0-239 visible
                if (scan.Point >= 1 && scan.Point <= 256) {
                    core.RenderStep(scan.Point - 1, scan.Line);
                }
                if (scan.Point >= 257 && scan.Point <= 320) {
                    core.ComputeSpriteForScanline(scan.Point - 257, scan.Line + 1);
                }
            }

            if (scan.Line == 241 && scan.Point == 1) {
                r.PpuStat.VBlank = true;
                if (r.PpuCtrl.NMIAtVBI) {
                    cpuMemory.Nmi = true;
                }
            }
            if (scan.Point == 0 && scan.Line >= 0 && scan.Line <= scan.LastScanline) {
                PPUAddressSpace.Instance.NotifyScanline();
            }
            scan++;
        }

        public void ClearVideoState() => PPUAddressSpace.Instance.ClearVideoState();

        public void WarmBoot() => ColdBoot();

        public void ColdBoot() {

            r.OnColdBoot();
            scan.Reset();
        }

        private class Scan {

            private int x, y, frame;
            private int lastScanline;
            private bool Even => (frame & 1) == 0;

            public int Point => x;
            public int Line => y;
            public int Frame => frame;
            internal int LastScanline => lastScanline;

            public Scan(int lastScanline) { this.lastScanline = lastScanline; }

            internal void SetLastScanline(int n) { lastScanline = n; }

            public static Scan operator ++(Scan scan) {
                int lastX = scan.y == -1 && scan.Even ? 340 : 339;
                scan.x++;
                if (scan.x > lastX) {
                    scan.x = 0; scan.y++;
                    if (scan.y > scan.lastScanline) {
                        scan.y = -1; scan.frame++;
                    }
                }
                return scan;
            }
            public void Reset() { x = 0; y = 0; frame = 0; }
        }
    }
}