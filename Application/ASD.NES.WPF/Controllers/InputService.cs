using System.Windows.Input;
using System.Windows.Threading;

namespace ASD.NES.WPF.Controllers {

    using Core;

    internal sealed class KeyboardController : IController {

        private Dispatcher dispatcher;
        public KeyboardController(Dispatcher dispatcher)
            => this.dispatcher = dispatcher;

        public bool IsKeyDown(JoypadKey key) {

            switch (key) {
                case JoypadKey.Left: return IsKeyDown(Key.A);
                case JoypadKey.Up: return IsKeyDown(Key.W);
                case JoypadKey.Right: return IsKeyDown(Key.D);
                case JoypadKey.Down: return IsKeyDown(Key.S);
                case JoypadKey.Select: return IsKeyDown(Key.RightShift);
                case JoypadKey.Start: return IsKeyDown(Key.Enter);
                case JoypadKey.B: return IsKeyDown(Key.K);
                case JoypadKey.A: return IsKeyDown(Key.L);
            }
            return false;
        }

        private bool IsKeyDown(Key key)
            => dispatcher.Invoke(() => Keyboard.IsKeyDown(key));
    }
}