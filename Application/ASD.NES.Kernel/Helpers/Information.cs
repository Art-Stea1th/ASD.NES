using System;

namespace ASD.NES.Kernel.Helpers {

    internal sealed class Information : Exception {
        public Information(string message) : base(message) { }
    }
}