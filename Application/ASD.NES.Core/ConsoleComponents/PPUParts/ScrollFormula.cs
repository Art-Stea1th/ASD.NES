namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    /// <summary>
    /// NESDEV-style background scroll: (StartX,StartY) + (scroll.X, scroll.Y) + (scanpoint, scanline)
    /// in 512x480 virtual space; nametable index and map coordinates for fetching the tile.
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
            out int nametableIndex,
            out int mapX,
            out int mapY) {

            var absX = (startX + scrollX + scanpoint) % VirtualWidth;
            var absY = (startY + scrollY + scanline) % VirtualHeight;

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
