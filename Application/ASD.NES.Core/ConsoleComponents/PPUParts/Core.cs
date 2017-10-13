using System;
using System.Collections.Generic;
using System.Text;

namespace ASD.NES.Core.ConsoleComponents.PPUParts {

    using Helpers;
    using Shared;

    internal sealed class Core {

        // PPU Address-Space 16kb

        // on cartridge 8kb (2x4kb) CHR-ROM

        private RInt8[] tileSet0 = new RInt8[4096];          // $0000-$0FFF   4k   CHR-ROM Tite-set0        (Board)
        private RInt8[] tileSet1 = new RInt8[4096];          // $1000-$1FFF   4k   CHR-ROM Tite-set1        (Board)


        // on console 2kb VRAM - (2 page\nametable x 1kb) - (and 2 page swap??)
        // Total 4 page\nametable x 1kb ($2000-2FFF)

        private RInt8[] vramPage1Symbols = new RInt8[960];   // $2000-$23BF   960b nametable1 – Symbols    (Console)
        private RInt8[] vramPage1Attributes = new RInt8[64]; // $23C0-$23FF   64b  nametable1 – Attributes (Console)

        private RInt8[] vramPage2Symbols = new RInt8[960];   // $2400-$26BF   960b nametable2 – Symbols    (Console)
        private RInt8[] vramPage2Attributes = new RInt8[64]; // $27C0-$27FF   64b  nametable2 – Attributes (Console)

        private RInt8[] vramPage3Symbols = new RInt8[960];   // $2800-$2BBF   960b nametable3 – Symbols    (Console)
        private RInt8[] vramPage3Attributes = new RInt8[64]; // $2BC0-$2BFF   64b  nametable3 – Attributes (Console)

        private RInt8[] vramPage4Symbols = new RInt8[960];   // $2C00-$2FBF   960b nametable4 – Symbols    (Console)
        private RInt8[] vramPage4Attributes = new RInt8[64]; // $2FC0-$2FFF   64b  nametable4 – Attributes (Console)

        // $3000-$3EFF   3840b  page\nametable Mirror of $2000-2EFF ~ x0.99 Overlap by palettes??

        // Total palette 64, at the same time 16 any colors for BG & 16 any colors for FG

        private RInt8[] backgroundPalette = new RInt8[16];   // $3F00-$3F0F   16b  BackgroundPalette (PPU Registers)
        private RInt8[] spritesPalette = new RInt8[16];      // $3F10-$3F1F   16b  SpritesPalette    (PPU Registers)

        // $3F20-$3FFF   224b   Palettes Mirror  (32b)  x7 ??
        // $4000-$4FFF   4092b  Mirrors of Above (1366) x3 ??

        // -------------------------------------------

        // - picture 256x240
        //
        // layers:
        //
        // BG      - fill by color &0 from BG palette
        // SPR     - bit priority 0
        // BG pic. - 32x30 icons, where 8x8 icon size // 32x30 = 960 numbers of icons stored in screen-page
        // SPR     - bit priority 1
    }
}