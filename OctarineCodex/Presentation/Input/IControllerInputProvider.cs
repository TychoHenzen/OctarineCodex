using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OctarineCodex.Application.Services;

namespace OctarineCodex.Presentation.Input;

/// <summary>
///     Abstraction for gamepad/controller input polling suitable for game controls.
///     Provides gamepad state access for the input system.
/// </summary>
[Service<DesktopControllerInputProvider>]
public interface IControllerInputProvider
{
    /// <summary>
    ///     Returns true if a controller is currently connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    ///     Gets the left thumbstick position as a Vector2 (-1 to 1 range).
    /// </summary>
    Vector2 LeftThumbstick { get; }

    /// <summary>
    ///     Gets the right thumbstick position as a Vector2 (-1 to 1 range).
    /// </summary>
    Vector2 RightThumbstick { get; }

    /// <summary>
    ///     Updates the controller state for the current frame.
    /// </summary>
    void Update(GameTime gameTime);

    /// <summary>
    ///     Returns true if the specified button is currently being held down.
    /// </summary>
    bool IsButtonDown(Buttons button);

    /// <summary>
    ///     Returns true if the specified button was just pressed this frame (not held from previous frame).
    /// </summary>
    bool IsButtonPressed(Buttons button);
}