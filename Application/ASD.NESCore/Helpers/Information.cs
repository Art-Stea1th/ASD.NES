using System;

namespace ASD.NESCore.Helpers {

    internal sealed class Information : Exception {
        public Information(string message) : base(message) { }
    }
}