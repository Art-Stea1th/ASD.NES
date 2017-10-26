namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    using CPUParts;
    using Shared;

    internal sealed class PPUCore {

        private uint[] palette = new uint[] {
            0x646464,0x003A88,0x2C2AA3,0x440093,0x670071,0x6E0040,0x6C0700,0x6A2102,0x503000,0x0C4800,0x004C00,0x004F08,0x00436C,0x000000,0x000000,0x000000,
            0xACACAC,0x005AD4,0x3042E1,0x8022E8,0xBB1EA5,0xCB346E,0xC63411,0xB04F00,0xA56C02,0x4C8A04,0x009000,0x008250,0x007BB5,0x242424,0x000000,0x000000,
            0xE4E4E4,0x4BA0FF,0x7096EF,0xBD72FF,0xD966D3,0xE4689A,0xF47658,0xFF970A,0xECBC00,0x91CB00,0x4DD149,0x00C686,0x3BB5DE,0x494949,0x000000,0x000000,
            0xFCFCFC,0xB6D8FF,0xB8CBF7,0xE3B8F7,0xF7B5F4,0xFAB7CD,0xF1C3B4,0xFACF82,0xFCDF86,0xDFF49E,0xADFAAD,0xA9FCD3,0xA6E8FF,0xDEDEDE,0x000000,0x000000,
        };

        private CPUAddressSpace cpuMemory = CPUAddressSpace.Instance;
        private PPUAddressSpace ppuMemory = PPUAddressSpace.Instance;

        private RegistersPPU r;
        private int spriteCount;

        private byte[] oam = new byte[256];
        private byte[] oamIndexes = new byte[8]; // tmp
        private uint[] frame = new uint[256 * 240];

        public uint[] Frame => frame;

        public PPUCore(RegistersPPU registers) {
            r = registers;
            r.OAMDMAWritten += FillOAM;
        }

        // BAD SMELLING BIG HARDCODE METHOD
        internal void RenderStep(int scanpoint, int scanline) {

            var backgroundCHR = default(byte);

            if (r.PpuMask.RenderBackground) {

                if (scanpoint >= 8 || r.PpuMask.RenderLeftmostBG) {

                    var absX = (r.PpuCtrl.StartX + r.PpuScrl.X + scanpoint) % 512;
                    var absY = (r.PpuCtrl.StartY + r.PpuScrl.Y + scanline) % 480;

                    var nametableIndex = 0;

                    int mapX = absX, mapY = absY;

                    if (absX >= 256) { mapX -= 256; nametableIndex |= 0b01; } // r.PpuCtrl.Add256ToX = true; // tmp. until cpu-ppu - out of sync
                    if (absY >= 240) { mapY -= 240; nametableIndex |= 0b10; } // r.PpuCtrl.Add240ToY = true; // tmp. until cpu-ppu - out of sync

                    var nametable = ppuMemory.GetNametable(nametableIndex);

                    int symbolX = mapX >> 3, symbolY = mapY >> 3;

                    var backgroundTileNumber = nametable.GetSymbol(symbolX, symbolY);

                    var backgroundColorBitsL = GetTileBits(backgroundTileNumber, mapY & 7, mapX & 7, r.PpuCtrl.BackgroundPatternTableAddress);

                    var attribute = nametable.GetAttribute(mapX >> 5, mapY >> 5);

                    var quadrantOffset = ((symbolY & 0b10) << 1 | (symbolX & 0b10)); // 0, 2, 4, 6
                    var backgroundColorBitsH = (byte)((attribute >> quadrantOffset) & 0b11);

                    // ---- oooo ----

                    if (backgroundColorBitsL == 0) { backgroundColorBitsH = 0; }

                    var backgroundPaletteBaseAddress = 0x3F00;
                    var backgroundColorIndex = (backgroundColorBitsH << 2) | backgroundColorBitsL;
                    var paletteColorIndex = ppuMemory[backgroundPaletteBaseAddress + backgroundColorIndex]; // 0 - 63

                    var pixelColorRGB = palette[paletteColorIndex];

                    frame[Linearize(scanpoint, scanline, 256)] = pixelColorRGB; // write pixel

                    backgroundCHR = backgroundColorBitsL;    // tmp save
                }                
            }

            if (r.PpuMask.RenderSprites) {

                if (scanpoint >= 8 || r.PpuMask.RenderLeftmostSpr) {

                    var linearIndex = Linearize(scanpoint, scanline, 256);

                    for (var spriteIndex = 0; spriteIndex < spriteCount; spriteIndex++) { // Check each sprite (spriteCount 0-8);

                        if (scanpoint < 8 && !r.PpuMask.RenderAll) { continue; }

                        int oamIndex = oamIndexes[spriteIndex];

                        var spriteY = (byte)(oam[oamIndex * 4 + 0] + 1);
                        var spriteTileNumber = oam[oamIndex * 4 + 1];
                        var attributes = (byte)(oam[oamIndex * 4 + 2] & 0xE3); // & 0xE3 - Uniplemented bits (by spec.)
                        var spriteX = oam[oamIndex * 4 + 3];

                        if (spriteY == 0 || spriteY >= 240) { continue; } // sprites never drawn if y-coord is 0
                        if (scanpoint < spriteX || scanpoint - spriteX >= 8) { continue; } // check range X

                        var spriteColorBitsH = (byte)(attributes & 0b11);
                        var inFrontOfBG = (attributes & 0x20) == 0;
                        var flipHorizontal = (attributes & 0x40) != 0;
                        var flipVertical = (attributes & 0x80) != 0;

                        var spriteColorBitsL = default(byte);

                        if (r.PpuCtrl.SpriteSizeY < 16) {

                            var titeX = scanpoint - spriteX;
                            var tileY = scanline - spriteY;

                            var spriteTileByte = flipVertical ? tileY ^ 7 : tileY;
                            var spriteTileBit = flipHorizontal ? titeX ^ 7 : titeX;

                            spriteColorBitsL = GetTileBits(spriteTileNumber, spriteTileByte, spriteTileBit, r.PpuCtrl.SpritePatternTableAddress);
                        }
                        else {

                            var whichTile = 0;
                            if (scanline - spriteY >= 8) { whichTile = 1; }
                            if (flipVertical) { whichTile ^= 1; }

                            var titeX = scanpoint - spriteX;
                            var tileY = (scanline - spriteY) & 7; // tile mirroring (ex. F to 7)

                            var spriteTileByte = flipVertical ? tileY ^ 7 : tileY;
                            var spriteTileBit = flipHorizontal ? titeX ^ 7 : titeX;

                            var spriteTilesBase = (spriteTileNumber & 1) << 12;
                            spriteTileNumber = (byte)((spriteTileNumber & 0xFE) + whichTile);

                            spriteColorBitsL = GetTileBits(spriteTileNumber, spriteTileByte, spriteTileBit, spriteTilesBase);
                        }

                        if (spriteColorBitsL > 0) {

                            if (inFrontOfBG || backgroundCHR == 0) {

                                var spriteZeroHit = oamIndex == 0 && backgroundCHR != 0;
                                if (r.PpuMask.RenderBackground && spriteZeroHit) {
                                    r.PpuStat.SpriteZeroHit = true;
                                }

                                var spritePaletteBase = 0x3F10;
                                var spriteColorIndex = spriteColorBitsH << 2 | spriteColorBitsL;
                                var spritePaletteColorIndex = ppuMemory[spritePaletteBase + spriteColorIndex];

                                var pixelColorRGB = palette[spritePaletteColorIndex];

                                frame[linearIndex] = pixelColorRGB;
                            }
                        }
                    }
                }                
            }
        }

        private byte GetTileBits(int tileNumber, int tileByte, int tileBit, int tilesBase) {

            var tileByteIndex = Linearize(tileByte, tileNumber, 16);

            var tileByteBitsL = ppuMemory[tilesBase + tileByteIndex];
            var tileByteBitsH = ppuMemory[tilesBase + tileByteIndex + 8];

            var tileBitL = (byte)((tileByteBitsL >> (tileBit ^ 7)) & 1);
            var tileBitH = (byte)((tileByteBitsH >> (tileBit ^ 7)) & 1);

            return (byte)((tileBitH << 1) | tileBitL);
        }

        /// <summary> return y * width + x (convert to single dimension index) </summary>
        private int Linearize(int x, int y, int width) => y * width + x;

        /// <summary> Cycles 257-320: Sprite fetches(8 sprites total, 8 cycles per sprite) <para/>
        /// 1-4: Read the Y-coordinate, tile number, attributes, and X-coordinate of the selected sprite from secondary OAM <para/>
        /// 5-8: Read the X-coordinate of the selected sprite from secondary OAM 4 times(while the PPU fetches the sprite tile data) <para/>
        /// For the first empty sprite slot, this will consist of sprite #63's Y-coordinate followed by 3 $FF bytes; for subsequent empty sprite slots, this will be four $FF bytes <para/>
        /// </summary>
        internal void ComputeSpriteForScanline(int scanpoint, int scanline) {

            if (scanpoint == 0) { spriteCount = 0; }

            var oamIndex = (byte)scanpoint;

            if (spriteCount < 8) {

                var spriteYStart = (byte)(oam[oamIndex * 4] + 1);
                var spriteYEnd = spriteYStart + r.PpuCtrl.SpriteSizeY;

                if (scanline >= spriteYStart && (scanline < spriteYEnd)) {
                    oamIndexes[spriteCount] = oamIndex;
                    spriteCount++;
                }
            }
        }

        private void FillOAM(Octet oamDmaAddress) {
            for (var offset = 0; offset < 0x100; ++offset) {
                var readAddress = (oamDmaAddress << 8) + ((offset + r.OamAddr.Value) & 0xFF);
                oam[offset] = cpuMemory[readAddress];
            }
        }
    }
}