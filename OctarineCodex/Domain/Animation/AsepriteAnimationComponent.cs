// OctarineCodex/Domain/Animation/AsepriteAnimationComponent.cs

using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Animation component that plays Aseprite-based animations
///     Integrates with existing IAnimationComponent interface.
/// </summary>
public class AsepriteAnimationComponent(
    Dictionary<string, AsepriteAnimation> animations,
    AsepriteAnimationData spriteData)
    : IAnimationComponent
{
    private AsepriteAnimation? _currentAnimation;
    private int _currentFrameIndex;
    private float _elapsedTime;

    public string CurrentAnimation => CurrentState;

    /// <summary>
    ///     Gets the current frame data for rendering.
    /// </summary>
    public AsepriteFrame CurrentFrameData
    {
        get
        {
            if (_currentAnimation == null || _currentFrameIndex >= _currentAnimation.Frames.Count)
            {
                return new AsepriteFrame(
                    new AsepriteRect(0, 0, 16, 32),
                    false,
                    false,
                    new AsepriteRect(0, 0, 16, 32),
                    new AsepriteSize(16, 32),
                    100);
            }

            return _currentAnimation.Frames[_currentFrameIndex];
        }
    }

    public bool IsComplete { get; private set; }
    public bool IsPlaying { get; private set; } = true;

    public string CurrentState => _currentAnimation?.Name ?? string.Empty;

    public void Update(GameTime gameTime)
    {
        if (!IsPlaying || _currentAnimation == null || _currentAnimation.Frames.Count == 0)
        {
            return;
        }

        _elapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        AsepriteFrame currentFrame = _currentAnimation.Frames[_currentFrameIndex];
        if (_elapsedTime < currentFrame.Duration)
        {
            return;
        }

        _elapsedTime -= currentFrame.Duration;
        _currentFrameIndex++;

        if (_currentFrameIndex < _currentAnimation.Frames.Count)
        {
            return;
        }

        if (_currentAnimation.Loop)
        {
            _currentFrameIndex = 0; // Loop back to start
        }
        else
        {
            _currentFrameIndex = _currentAnimation.Frames.Count - 1;
            IsComplete = true;
            IsPlaying = false;
        }
    }

    public int GetCurrentFrame()
    {
        return _currentFrameIndex;
    }

    public int GetCurrentTileId()
    {
        // For compatibility with existing tile-based system
        // This would need to be calculated based on the sprite sheet layout
        AsepriteRect frame = CurrentFrameData.Frame;
        var tileWidth = 16; // This should come from configuration
        var tilesPerRow = spriteData.Meta.Size.W / tileWidth;

        return (frame.Y / frame.H * tilesPerRow) + (frame.X / frame.W);
    }

    public void PlayAnimation(string animationName)
    {
        if (!animations.TryGetValue(animationName, out AsepriteAnimation? animation))
        {
            return;
        }

        _currentAnimation = animation;
        _currentFrameIndex = 0;
        _elapsedTime = 0f;
        IsPlaying = true;
        IsComplete = false;
    }

    public void StopAnimation()
    {
        IsPlaying = false;
    }
}
