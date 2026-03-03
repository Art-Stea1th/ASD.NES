using System;
using System.Windows.Input;

namespace ASD.NES.WPF.Models {

    /// <summary>NES controller: D-pad (Up, Down, Left, Right), Select, Start, A, B. No X/Y (SNES).</summary>
    public sealed class ControllerMappingModel {

        public string Left { get; set; }
        public string Up { get; set; }
        public string Right { get; set; }
        public string Down { get; set; }
        public string Select { get; set; }
        public string Start { get; set; }
        public string B { get; set; }
        public string A { get; set; }

        public static ControllerMappingModel FromKeys(Key left, Key up, Key right, Key down, Key select, Key start, Key b, Key a) {
            return new ControllerMappingModel {
                Left = left.ToString(),
                Up = up.ToString(),
                Right = right.ToString(),
                Down = down.ToString(),
                Select = select.ToString(),
                Start = start.ToString(),
                B = b.ToString(),
                A = a.ToString()
            };
        }

        public void ToKeys(out Key left, out Key up, out Key right, out Key down, out Key select, out Key start, out Key b, out Key a) {
            left = ParseKey(Left, Key.A);
            up = ParseKey(Up, Key.W);
            right = ParseKey(Right, Key.D);
            down = ParseKey(Down, Key.S);
            select = ParseKey(Select, Key.LeftShift);
            start = ParseKey(Start, Key.Enter);
            b = ParseKey(B, Key.K);
            a = ParseKey(A, Key.L);
        }

        private static Key ParseKey(string value, Key defaultKey) {
            if (string.IsNullOrEmpty(value)) {
                return defaultKey;
            }
            try {
                return (Key)Enum.Parse(typeof(Key), value, true);
            }
            catch {
                return defaultKey;
            }
        }
    }
}
