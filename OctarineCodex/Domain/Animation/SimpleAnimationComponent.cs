// Domain/Animation/SimpleAnimationComponent.cs

using Microsoft.Xna.Framework;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Basic animation component for simple looping animations like tiles.
///     Supports magic system integration for dynamic behavior.
/// </summary>
public class SimpleAnimationComponent : IAnimationComponent
{
    private LDtkAnimationData _animationData;
    private int _currentFrame;
    private float _elapsedTime;

    public bool IsComplete { get; private set; }
    public bool IsPlaying { get; private set; } = true;

    public string CurrentState => _animationData.Name;

    public void Update(GameTime gameTime)
    {
        if (!IsPlaying || _animationData.FrameTileIds.Length == 0)
        {
            return;
        }

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var effectiveFrameRate = GetEffectiveFrameRate();

        _elapsedTime += deltaTime * effectiveFrameRate;

        var frameTime = 1f / effectiveFrameRate;
        if (_elapsedTime >= frameTime)
        {
            _elapsedTime -= frameTime;
            _currentFrame++;

            if (_currentFrame >= _animationData.FrameTileIds.Length)
            {
                if (_animationData.Loop)
                {
                    _currentFrame = 0;
                }
                else
                {
                    _currentFrame = _animationData.FrameTileIds.Length - 1;
                    IsComplete = true;
                    IsPlaying = false;
                }
            }
        }
    }

    public int GetCurrentFrame()
    {
        return _currentFrame;
    }

    public int GetCurrentTileId()
    {
        if (_animationData.FrameTileIds.Length == 0)
        {
            return 0;
        }

        return _animationData.FrameTileIds[_currentFrame];
    }

    public void PlayAnimation(string animationName)
    {
        // For simple animations, this just restarts the current animation
        _elapsedTime = 0f;
        _currentFrame = 0;
        IsPlaying = true;
        IsComplete = false;
    }

    public void StopAnimation()
    {
        IsPlaying = false;
    }

    public void SetAnimation(LDtkAnimationData animationData)
    {
        _animationData = animationData;
        _elapsedTime = 0f;
        _currentFrame = 0;
        IsComplete = false;
        IsPlaying = true;
    }

    private float GetEffectiveFrameRate()
    {
        var baseRate = _animationData.FrameRate;

        return baseRate;
    }
}
