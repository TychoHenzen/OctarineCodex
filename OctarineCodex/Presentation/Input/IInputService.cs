using Microsoft.Xna.Framework;
using OctarineCodex.Application.Services;

namespace OctarineCodex.Presentation.Input;

/// <summary>
///     General-purpose input service that abstracts input from various sources (keyboard, controller, etc.).
///     Provides common game input actions without exposing specific input device details.
/// </summary>
[Service<CompositeInputService>]
public interface IInputService
{
    /// <summary>
    ///     Updates the input state for the current frame.
    ///     Should be called once per frame before querying input state.
    /// </summary>
    void Update(GameTime gameTime);

    /// <summary>
    ///     Gets the current movement input as a normalized direction vector.
    ///     Combines all active movement inputs from all sources.
    /// </summary>
    Vector2 GetMovementDirection();

    /// <summary>
    ///     Returns true if any exit/back action is currently pressed.
    /// </summary>
    bool IsExitPressed();

    /// <summary>
    ///     Returns true if the primary action button was just pressed this frame.
    /// </summary>
    bool IsPrimaryActionPressed();

    /// <summary>
    ///     Returns true if the secondary action button was just pressed this frame.
    /// </summary>
    bool IsSecondaryActionPressed();
}