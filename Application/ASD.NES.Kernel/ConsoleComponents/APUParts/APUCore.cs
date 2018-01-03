using System;
using System.Collections.Generic;
using System.Text;

namespace ASD.NES.Kernel.ConsoleComponents.APUParts {

    internal sealed class APUCore {

        private RegistersAPU r;

        public APUCore(RegistersAPU registers) {
            r = registers;
        }
    }
}