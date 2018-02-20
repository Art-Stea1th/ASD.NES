using System.Windows.Input;
using System.Windows.Threading;

namespace ASD.NES.WPF.Controllers {

    using Core;
    using Helpers;

    internal sealed class KeyboardController : Observable, IGamepad {

        private Dispatcher dispatcher;
        private Key left, up, right, down, select, start, b, a;

        public Key Left { get => left; set => Set(ref left, value); }
        public Key Up { get => up; set => Set(ref up, value); }
        public Key Right { get => right; set => Set(ref right, value); }
        public Key Down { get => down; set => Set(ref down, value); }
        public Key Select { get => select; set => Set(ref select, value); }
        public Key Start { get => start; set => Set(ref start, value); }
        public Key B { get => b; set => Set(ref b, value); }
        public Key A { get => a; set => Set(ref a, value); }

        public KeyboardController(Dispatcher dispatcher)
            => this.dispatcher = dispatcher;

        bool IGamepad.IsKeyDown(GamepadKey key) {

            switch (key) {
                case GamepadKey.Left: return IsKeyDown(left);
                case GamepadKey.Up: return IsKeyDown(up);
                case GamepadKey.Right: return IsKeyDown(right);
                case GamepadKey.Down: return IsKeyDown(down);
                case GamepadKey.Select: return IsKeyDown(select);
                case GamepadKey.Start: return IsKeyDown(start);
                case GamepadKey.B: return IsKeyDown(b);
                case GamepadKey.A: return IsKeyDown(a);
            }
            return false;
        }

        private bool IsKeyDown(Key key)
            => dispatcher.Invoke(() => Keyboard.IsKeyDown(key));
    }
}