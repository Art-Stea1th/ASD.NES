namespace ASD.NES.Core {

    public interface IController {
        bool IsKeyDown(JoypadKey key);
    }

    public enum JoypadKey {
        Left, Up, Right, Down,
        Select, Start,
        B, A
    }
}