using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OctarineCodex.Input;

/// <summary>
/// DesktopGL implementation of IControllerInputProvider.
/// Uses GamePad.GetState() to poll controller input for Player One.
/// </summary>
public sealed class DesktopControllerInputProvider : IControllerInputProvider
{
    private GamePadState _currentState;
    private GamePadState _previousState;
    private const PlayerIndex PlayerIndex = Microsoft.Xna.Framework.PlayerIndex.One;

    public void Update(GameTime gameTime)
    {
        _previousState = _currentState;
        _currentState = GamePad.GetState(PlayerIndex);
    }

    public bool IsConnected => _currentState.IsConnected;

    public Vector2 LeftThumbstick => _currentState.ThumbSticks.Left;

    public Vector2 RightThumbstick => _currentState.ThumbSticks.Right;

    public bool IsButtonDown(Buttons button) => _currentState.IsButtonDown(button);

    public bool IsButtonPressed(Buttons button) => 
        _currentState.IsButtonDown(button) && !_previousState.IsButtonDown(button);
}