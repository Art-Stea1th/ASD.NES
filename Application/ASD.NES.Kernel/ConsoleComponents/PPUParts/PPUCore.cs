using System.Threading.Tasks;

namespace ASD.NES.Kernel.ConsoleComponents.PPUParts {
    using System.Linq;
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

        private ObjectAttributeMemory oam = new ObjectAttributeMemory();
        private byte[] oamIndexes = new byte[64]; // set 64 :) original NES spr.limit = 8; // TODO: add this as option
        private uint[] frame = new uint[256 * 240];

        public uint[] Frame => frame;

        public PPUCore(RegistersPPU registers) {
            r = registers;
            r.OAMDMAWritten += FillOAM;
        }

        internal void RenderStep(int scanpoint, int scanline) {

            var backgroundColorIndex = default(byte);

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
                    var backgroundColorBitsH = 0;

                    if (backgroundColorBitsL > 0) {

                        var attribute = nametable.GetAttribute(mapX >> 5, mapY >> 5);
                        var quadrantOffset = ((symbolY & 0b10) << 1 | (symbolX & 0b10)); // 0, 2, 4, 6

                        backgroundColorBitsH = (byte)((attribute >> quadrantOffset) & 0b11);
                    }
                    backgroundColorIndex = (byte)(backgroundColorBitsH << 2 | backgroundColorBitsL);
                }
            }

            var spriteColorIndex = default(byte);
            var spriteInFront = default(bool);

            if (r.PpuMask.RenderSprites) {

                if (scanpoint >= 8 || r.PpuMask.RenderLeftmostSpr) {

                    for (var spriteIndex = 0; spriteIndex < spriteCount; spriteIndex++) { // Check each sprite (spriteCount 0-8-64);

                        if (scanpoint < 8 && !r.PpuMask.RenderAll) { continue; }

                        int oamIndex = oamIndexes[spriteIndex];
                        var sprite = oam[oamIndex]; // oamEntry

                        if (scanline < sprite.Y || scanline - sprite.Y >= r.PpuCtrl.SpriteSizeY) { continue; }
                        if (scanpoint < sprite.X || scanpoint - sprite.X >= r.PpuCtrl.SpriteSizeX) { continue; }


                        var spriteTileNumber = sprite.TileNumber;
                        var spriteTileByte = GetInTileCoordinate(scanline, sprite.Y, sprite.FlipV);
                        var spriteTileBit = GetInTileCoordinate(scanpoint, sprite.X, sprite.FlipH);
                        var spriteTilesBase = r.PpuCtrl.SpritePatternTableAddress;

                        if (r.PpuCtrl.SpriteSize16) {

                            var whichTile = 0;
                            if (scanline - sprite.Y >= 8) { whichTile ^= 1; }
                            if (sprite.FlipV) { whichTile ^= 1; }

                            spriteTilesBase = (sprite.TileNumber & 1) << 12;
                            spriteTileNumber = (byte)((sprite.TileNumber & 0xFE) + whichTile);
                        }

                        var spriteColorBitsL = GetTileBits(spriteTileNumber, spriteTileByte, spriteTileBit, spriteTilesBase);                        

                        if (spriteColorBitsL > 0) {

                            spriteInFront = sprite.InFront;
                            if (spriteInFront || backgroundColorIndex == 0) {

                                if (oamIndex == 0 && r.PpuMask.RenderBackground/* && backgroundColorIndex != 0*/) {
                                    r.PpuStat.SpriteZeroHit = true;
                                }
                                spriteColorIndex = (byte)(sprite.ColorBitsH << 2 | spriteColorBitsL);
                            }
                        }
                    }
                }
            }
            frame[Linearize(scanpoint, scanline, 256)] = palette[GetPaletteIndex(backgroundColorIndex, spriteColorIndex, spriteInFront)];
        }

        private Octet GetPaletteIndex(Octet bckgColorBit, Octet sprtColorBit, bool sprInFront) { // Multiplexer

            var paletteAddress = 0x3F00; // 0x3F00 - background, 0x3F10 - sprite
            var colorIndex = bckgColorBit;

            if ((sprtColorBit & 0b0011) > 0) {
                if (sprInFront || bckgColorBit == 0) {
                    paletteAddress |= 0x10;
                    colorIndex = sprtColorBit;
                }
            }
            return ppuMemory[paletteAddress + colorIndex]; // palette index
        }

        private int GetInTileCoordinate(int scanCoordinate, int spriteCoordinate, bool flip) {
            var coord = (scanCoordinate - spriteCoordinate) & 7; // 'AND 7' - tile mirroring, important for Y in 8x16 mode
            return flip ? coord ^ 7 : coord;
        }

        private byte GetTileBits(int tileNumber, int tileByte, int tileBit, int tilesBase) {

            var tileByteIndex = Linearize(tileByte, tileNumber, 16);

            var tileByteBitsL = ppuMemory[tilesBase + tileByteIndex];
            var tileByteBitsH = ppuMemory[tilesBase + tileByteIndex + 8];

            var tileBitL = (byte)((tileByteBitsL >> (tileBit ^ 7)) & 1);
            var tileBitH = (byte)((tileByteBitsH >> (tileBit ^ 7)) & 1);

            return (byte)((tileBitH << 1) | tileBitL); // 2bit result
        }

        private int Linearize(int x, int y, int width) => y * width + x;

        internal void ComputeSpriteForScanline(int scanpoint, int scanline) {

            if (scanpoint == 0) { spriteCount = 0; }

            var oamIndex = (byte)scanpoint;

            if (spriteCount < oamIndexes.Length) {

                var spriteYStart = oam[oamIndex].Y;
                var spriteYEnd = spriteYStart + r.PpuCtrl.SpriteSizeY;

                if (scanline >= spriteYStart && (scanline < spriteYEnd)) {
                    oamIndexes[spriteCount] = oamIndex;
                    spriteCount++;
                }
            }
        }

        private void FillOAM(Octet oamDmaAddress) {

            int GetCPUMemoryAddress(int offset) => (oamDmaAddress << 8) + ((r.OamAddr.Value + offset));

            for (var i = 0; i < oam.Cells; i++) {
                oam[i] = Quadlet.Make(
                    Hextet.Make(
                        cpuMemory[GetCPUMemoryAddress(i << 2 | 0b11)],
                        cpuMemory[GetCPUMemoryAddress(i << 2 | 0b10)]),
                    Hextet.Make(
                        cpuMemory[GetCPUMemoryAddress(i << 2 | 0b01)],
                        cpuMemory[GetCPUMemoryAddress(i << 2 | 0b00)]));
            }
        }
    }
}