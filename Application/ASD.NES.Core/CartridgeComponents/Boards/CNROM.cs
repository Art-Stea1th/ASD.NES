using System;
using System.Collections.Generic;

namespace ASD.NES.Core.CartridgeComponents.Boards {

    //https://wiki.nesdev.com/w/index.php/CNROM
    internal abstract class CNROM : Board {

        protected override byte Read(int address) {
            throw new NotImplementedException();
        }

        protected override void Write(int address, byte value) {
            throw new NotImplementedException();
        }

        public override void SetCHR(IReadOnlyList<byte[]> chr) {
            throw new NotImplementedException();
        }
        public override void SetPRG(IReadOnlyList<byte[]> prg) {
            throw new NotImplementedException();
        }
    }
}