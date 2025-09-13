// OctarineCodex/Domain/Animation/AsepriteAnimationComponent.cs

using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Animation component that plays Aseprite-based animations
///     Integrates with existing IAnimationComponent interface
/// </summary>
public class AsepriteAnimationComponent : IAnimationComponent
{
    private readonly Dictionary<string, AsepriteAnimation> _animations;
    private readonly AsepriteAnimationData _spriteData;
    private AsepriteAnimation? _currentAnimation;
    private int _currentFrameIndex;
    private float _elapsedTime;

    public AsepriteAnimationComponent(
        Dictionary<string, AsepriteAnimation> animations, AsepriteAnimationData spriteData)
    {
        _animations = animations;
        _spriteData = spriteData;
    }

    public string CurrentAnimation => CurrentState;

    public bool IsComplete { get; private set; }
    public bool IsPlaying { get; private set; } = true;

    public string CurrentState => _currentAnimation?.Name ?? "";

    public void Update(GameTime gameTime)
    {
        if (!IsPlaying || _currentAnimation == null || _currentAnimation.Frames.Count == 0)
        {
            return;
        }

        _elapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        AsepriteFrame currentFrame = _currentAnimation.Frames[_currentFrameIndex];
        if (_elapsedTime >= currentFrame.Duration)
        {
            _elapsedTime -= currentFrame.Duration;
            _currentFrameIndex++;

            if (_currentFrameIndex >= _currentAnimation.Frames.Count)
            {
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
        AsepriteFrame frame = GetCurrentFrameData();
        var tileWidth = 16; // This should come from configuration
        var tilesPerRow = _spriteData.Meta.Size.W / tileWidth;

        return (frame.Frame.Y / frame.Frame.H * tilesPerRow) + (frame.Frame.X / frame.Frame.W);
    }

    public void PlayAnimation(string animationName)
    {
        if (_animations.TryGetValue(animationName, out AsepriteAnimation? animation))
        {
            _currentAnimation = animation;
            _currentFrameIndex = 0;
            _elapsedTime = 0f;
            IsPlaying = true;
            IsComplete = false;
        }
    }

    public void StopAnimation()
    {
        IsPlaying = false;
    }

    /// <summary>
    ///     Gets the current frame data for rendering
    /// </summary>
    public AsepriteFrame GetCurrentFrameData()
    {
        if (_currentAnimation == null || _currentFrameIndex >= _currentAnimation.Frames.Count)
        {
            return new AsepriteFrame(new AsepriteRect(0, 0, 16, 32), false, false,
                new AsepriteRect(0, 0, 16, 32), new AsepriteSize(16, 32), 100);
        }

        return _currentAnimation.Frames[_currentFrameIndex];
    }
}
