namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    /// <summary>
    /// NESDEV-style background scroll: (StartX,StartY) + (scroll.X, scroll.Y) + (scanpoint, scanline).
    /// For H/V/FourScreen: 512x480 virtual space; nametable index and map coords for the tile.
    /// For SingleScreen (AxROM, etc.): one 256x240 nametable, wrap at 256/240 (no 2x2 quadrants).
    /// </summary>
    internal static class ScrollFormula {

        public const int VirtualWidth = 512;
        public const int VirtualHeight = 480;
        public const int NametableWidthPx = 256;
        public const int NametableHeightPx = 240;

        /// <summary>Compute background tile coordinates from scroll state. Returns nametable index (0-3) and pixel (mapX, mapY) within that nametable.</summary>
        public static void GetBackgroundCoords(
            int startX,
            int startY,
            int scrollX,
            int scrollY,
            int scanpoint,
            int scanline,
            Mirroring mirroring,
            out int nametableIndex,
            out int mapX,
            out int mapY) {

            if (mirroring == Mirroring.SingleScreen) {
                // Single-screen: one 256x240 nametable; $2000 nametable bits are ignored by hardware (all 4 point to same RAM).
                // Use only scroll + (scanpoint, scanline), wrap at 256/240 (NESDEV).
                var ax = scrollX + scanpoint;
                var ay = scrollY + scanline;
                mapX = ((ax % NametableWidthPx) + NametableWidthPx) % NametableWidthPx;
                mapY = ((ay % NametableHeightPx) + NametableHeightPx) % NametableHeightPx;
                nametableIndex = 0;
                return;
            }

            var vw = VirtualWidth;
            var vh = VirtualHeight;
            var absX = (startX + scrollX + scanpoint) % vw;
            var absY = (startY + scrollY + scanline) % vh;

            nametableIndex = 0;
            mapX = absX;
            mapY = absY;
            if (absX >= NametableWidthPx) {
                mapX = absX - NametableWidthPx;
                nametableIndex |= 1;
            }
            if (absY >= NametableHeightPx) {
                mapY = absY - NametableHeightPx;
                nametableIndex |= 2;
            }
        }
    }
}
