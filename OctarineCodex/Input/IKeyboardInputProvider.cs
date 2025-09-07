using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OctarineCodex.Input;

/// <summary>
///     Abstraction for keyboard input polling suitable for game controls.
///     Provides raw keyboard state access for the input system.
/// </summary>
[Service<DesktopKeyboardInputProvider>]
public interface IKeyboardInputProvider
{
    /// <summary>
    ///     Updates the keyboard state for the current frame.
    /// </summary>
    void Update(GameTime gameTime);

    /// <summary>
    ///     Returns true if the specified key is currently being held down.
    /// </summary>
    bool IsKeyDown(Keys key);

    /// <summary>
    ///     Returns true if the specified key was just pressed this frame (not held from previous frame).
    /// </summary>
    bool IsKeyPressed(Keys key);
}