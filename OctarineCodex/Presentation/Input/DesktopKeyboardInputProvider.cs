using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OctarineCodex.Presentation.Input;

/// <summary>
///     DesktopGL implementation of IKeyboardInputProvider.
///     Uses continuous polling via Keyboard.GetState() for smooth game input.
/// </summary>
public sealed class DesktopKeyboardInputProvider : IKeyboardInputProvider
{
    private KeyboardState _currentState;
    private KeyboardState _previousState;

    public void Update(GameTime gameTime)
    {
        _previousState = _currentState;
        _currentState = Keyboard.GetState();
    }

    public bool IsKeyDown(Keys key)
    {
        return _currentState.IsKeyDown(key);
    }

    public bool IsKeyPressed(Keys key)
    {
        return _currentState.IsKeyDown(key) && !_previousState.IsKeyDown(key);
    }
}