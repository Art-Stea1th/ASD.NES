namespace ASD.NES.Core.ConsoleComponents {

    using System;
    using CPUParts;
    using PPUParts;

    internal sealed class PictureProcessor {

        /// <summary> When set by the host, one line per frame: mirror, ctrl, mask, scroll, sample (nt,tile) at (0,0),(128,0),(255,0),(0,120),(0,239). </summary>
        internal static Action<string> PpuScrollLogLine;

        /// <summary> When set, one line per sample scanline (0,8,120,239): "sl=Y nt0=X t0=Z nt128=W t128=V". </summary>
        internal static Action<string> PpuScanlineLogLine;

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
                if (scan.Point == 1) {
                    if (PpuScrollLogLine != null) {
                        LogPpuScrollOncePerFrame();
                    }
                    if (PpuScanlineLogLine != null && (scan.Line == 0 || scan.Line == 8 || scan.Line == 120 || scan.Line == 239)) {
                        LogPpuScanline(scan.Line);
                    }
                }
                if (scan.Point >= 1 && scan.Point <= 256) {
                    core.RenderStep(scan.Point - 1, scan.Line, r.PpuScrl.X, r.PpuScrl.Y, r.PpuCtrl.StartX, r.PpuCtrl.StartY);
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

        private void LogPpuScrollOncePerFrame() {
            try {
                var ppu = PPUAddressSpace.Instance;
                var mirror = ppu.NametableMirroring;
                var mask = (byte)r.PpuMask;
                var ctrl = (byte)r.PpuCtrl;
                var sx = r.PpuScrl.X;
                var sy = r.PpuScrl.Y;
                var startX = r.PpuCtrl.StartX;
                var startY = r.PpuCtrl.StartY;

                ScrollFormula.GetBackgroundCoords(startX, startY, sx, sy, 0, 0, out int nt0, out int mx0, out int my0);
                ScrollFormula.GetBackgroundCoords(startX, startY, sx, sy, 128, 0, out int nt128, out int mx128, out int my128);
                ScrollFormula.GetBackgroundCoords(startX, startY, sx, sy, 255, 0, out int nt255, out int mx255, out int my255);
                ScrollFormula.GetBackgroundCoords(startX, startY, sx, sy, 0, 120, out int nt120, out int mx120, out int my120);
                ScrollFormula.GetBackgroundCoords(startX, startY, sx, sy, 0, 239, out int nt239, out int mx239, out int my239);

                var tile00 = ppu.GetNametable(nt0).GetSymbol(mx0 >> 3, my0 >> 3);
                var tile128 = ppu.GetNametable(nt128).GetSymbol(mx128 >> 3, my128 >> 3);
                var tile255 = ppu.GetNametable(nt255).GetSymbol(mx255 >> 3, my255 >> 3);
                var tileMid = ppu.GetNametable(nt120).GetSymbol(mx120 >> 3, my120 >> 3);
                var tileBot = ppu.GetNametable(nt239).GetSymbol(mx239 >> 3, my239 >> 3);

                var line = string.Format(
                    "F{0} mirror={1} ctrl={2:X2} mask={3:X2} scrX={4} scrY={5} | (0,0) nt{6} t{7:X2} | (128,0) nt{8} t{9:X2} | (255,0) nt{10} t{11:X2} | (0,120) nt{12} t{13:X2} | (0,239) nt{14} t{15:X2}",
                    scan.Frame, mirror, ctrl, mask, sx, sy,
                    nt0, tile00, nt128, tile128, nt255, tile255, nt120, tileMid, nt239, tileBot);
                PpuScrollLogLine(line);
            }
            catch (Exception) {
                // ignore log errors
            }
        }

        private void LogPpuScanline(int sl) {
            try {
                var ppu = PPUAddressSpace.Instance;
                var startX = r.PpuCtrl.StartX;
                var startY = r.PpuCtrl.StartY;
                var sx = r.PpuScrl.X;
                var sy = r.PpuScrl.Y;
                ScrollFormula.GetBackgroundCoords(startX, startY, sx, sy, 0, sl, out int nt0, out int mx0, out int my0);
                ScrollFormula.GetBackgroundCoords(startX, startY, sx, sy, 128, sl, out int nt128, out int mx128, out int my128);
                var t0 = ppu.GetNametable(nt0).GetSymbol(mx0 >> 3, my0 >> 3);
                var t128 = ppu.GetNametable(nt128).GetSymbol(mx128 >> 3, my128 >> 3);
                PpuScanlineLogLine(string.Format("sl={0} nt0={1} t0={2:X2} nt128={3} t128={4:X2}", sl, nt0, t0, nt128, t128));
            }
            catch (Exception) {
                // ignore
            }
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
                var lastX = scan.y == -1 && scan.Even ? 340 : 339;
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