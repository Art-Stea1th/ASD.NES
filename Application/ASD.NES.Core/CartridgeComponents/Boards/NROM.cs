using System;


namespace ASD.NES.Core.CartridgeComponents.Boards {

    // https://wiki.nesdev.com/w/index.php/NROM
    internal abstract class NROM : Board { // TODO: check prg/chr banks count before access by index

        protected override byte Read(int address) {

            if (address < 0x6000) {
                throw new IndexOutOfRangeException();
            }
            if (address < 0x8000) {
                return chr[0][address - 0x6000];
            }
            if (address < 0xC000) {
                return prg[1][address - 0x8000];
            }
            if (address < 0x10000) {
                return prg[0][address - 0xC000];
            }
            throw new IndexOutOfRangeException();
        }

        protected override void Write(int address, byte value) {

            if (address == 0xFFFA) {
                prg[0][address - 0xC000] = value;
            }
            else {
                throw new InvalidOperationException();
            }
        }
    }
}