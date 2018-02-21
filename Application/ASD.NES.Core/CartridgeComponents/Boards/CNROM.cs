using System;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    //https://wiki.nesdev.com/w/index.php/CNROM
    internal abstract class CNROM : Board { // TODO: Verify

        private int bank = 0;

        protected override byte Read(int address) {

            if (address < 0x2000) {
                return chr[bank][address];
            }
            if (address < 0xC000) {
                if (prg.Count > 1) {
                    return prg[1][address - 0x8000];
                }
                return prg[0][address - 0x8000];
            }
            if (address < 0x10000) {
                return prg[0][address - 0xC000];
            }

            throw new IndexOutOfRangeException();
        }

        protected override void Write(int address, byte value) {
            if (address >= 0x8000 && address <= 0xFFFF) {
                if (address >= 0xFFFA) {
                    prg[0][address - 0xC000] = value;
                }
                else {
                    bank = value & 3;
                }
            }
        }
    }
}