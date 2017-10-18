using System;
using System.Collections.Generic;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    using Helpers;
    using Shared;

    internal abstract class NROM : Board {

        private static RefOctet[] chrROM0; // $6000-$7FFF - 8 kb

        private static RefOctet[] prgROM1; // $8000-$BFFF - 16 kb NROM-256 additional
        private static RefOctet[] prgROM0; // $C000-$FFFF - 16 kb NROM-128

        public override int Cells => 1024 * 64;

        public override Octet Read(int address) {
            if (address < 0x6000) {
                throw new IndexOutOfRangeException();
            }
            if (address < 0x8000) {
                return chrROM0[address - 0x6000];
            }
            if (address < 0xC000) {
                return prgROM1[address - 0x8000];
            }
            if (address < 0x10000) {
                return prgROM0[address - 0xC000];
            }
            throw new IndexOutOfRangeException();
        }

        public override void Write(int address, Octet value) {
            throw new InvalidOperationException();
        }

        public override void SetCHR(IReadOnlyList<Octet[]> chr) {

            if (chr == null) { throw new ArgumentException(); }
            if (chr.Count < 1) { throw new ArgumentOutOfRangeException(); }

            chrROM0 = chr[0].Wrap();
        }

        public override void SetPRG(IReadOnlyList<Octet[]> prg) {

            if (prg == null) { throw new ArgumentException(); }
            if (prg.Count < 1) { throw new ArgumentOutOfRangeException(); }

            prgROM0 = prg[0].Wrap();

            if (prg.Count > 1) {
                prgROM1 = prg[1].Wrap();
            }
        }
    }
}