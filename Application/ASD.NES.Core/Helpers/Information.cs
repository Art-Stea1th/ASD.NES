using System;

namespace ASD.NES.Core.Helpers {

    internal sealed class Information : Exception {
        public Information(string message) : base(message) { }
    }
}