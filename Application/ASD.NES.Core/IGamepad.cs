namespace ASD.NES.Core {

    public interface IGamepad {
        bool IsKeyDown(GamepadKey key);
    }

    public enum GamepadKey { Left, Up, Right, Down, Select, Start, B, A }
}