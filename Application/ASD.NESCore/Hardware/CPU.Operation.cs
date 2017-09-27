namespace ASD.NESCore.Hardware {

    internal sealed partial class CPU {        

        private abstract class Operation {

            public readonly byte Code;
            public Operation(byte code)
                => Code = code;
        }
    }
}