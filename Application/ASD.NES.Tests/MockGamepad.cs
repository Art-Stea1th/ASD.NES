using ASD.NES.Core;

namespace ASD.NES.Tests;

/// <summary>
/// Mock gamepad for tests. Bit order matches Core InputPort MakeInt8: bit0=A, bit1=B, bit2=Select, bit3=Start, bit4=Up, bit5=Down, bit6=Left, bit7=Right.
/// </summary>
internal sealed class MockGamepad : IGamepad
{
    private byte _state;

    public byte State {
        get => _state;
        set => _state = value;
    }

    public bool IsKeyDown(GamepadKey key) {
        var bit = key switch {
            GamepadKey.A => 0,
            GamepadKey.B => 1,
            GamepadKey.Select => 2,
            GamepadKey.Start => 3,
            GamepadKey.Up => 4,
            GamepadKey.Down => 5,
            GamepadKey.Left => 6,
            GamepadKey.Right => 7,
            _ => 0
        };
        return (_state & (1 << bit)) != 0;
    }
}
