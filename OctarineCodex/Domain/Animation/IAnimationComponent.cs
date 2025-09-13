// Domain/Animation/IAnimationComponent.cs

using Microsoft.Xna.Framework;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Core interface for all animation components in OctarineCodex.
///     Supports both simple and layered animations with magic system integration.
/// </summary>
public interface IAnimationComponent
{
    /// <summary>
    ///     Whether this animation has completed (for non-looping animations).
    /// </summary>
    bool IsComplete { get; }

    /// <summary>
    ///     Whether this animation is currently playing.
    /// </summary>
    bool IsPlaying { get; }

    /// <summary>
    ///     Current animation state name (for state-based animations).
    /// </summary>
    string CurrentState { get; }

    /// <summary>
    ///     Updates animation timing and state.
    /// </summary>
    void Update(GameTime gameTime);

    /// <summary>
    ///     Gets the current frame index for this animation.
    /// </summary>
    int GetCurrentFrame();

    /// <summary>
    ///     Gets the current tile ID for tile-based animations.
    /// </summary>
    int GetCurrentTileId();

    /// <summary>
    ///     Triggers a specific animation by name.
    /// </summary>
    void PlayAnimation(string animationName);

    /// <summary>
    ///     Stops the current animation.
    /// </summary>
    void StopAnimation();
}
