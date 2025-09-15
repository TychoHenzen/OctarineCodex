// Domain/Animation/SimpleAnimationComponent.cs

using System;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Domain.Animation;

/// <summary>
/// Basic animation component for simple looping animations like tiles.
/// Supports magic system integration for dynamic behavior.
/// </summary>
public class SimpleAnimationComponent : IAnimationComponent
{
    private LDtkAnimationData _animationData;
    private int _currentFrame;
    private float _elapsedTime;

    public bool IsComplete { get; private set; }
    public bool IsPlaying { get; private set; } = true;

    public string CurrentState => _animationData.Name ?? "None";

    public void Update(GameTime gameTime)
    {
        if (!IsPlaying || _animationData.FrameTileIds == null || _animationData.FrameTileIds.Length == 0)
        {
            return;
        }

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Convert frame rate to frame time (time per frame)
        var frameTime = _animationData.FrameRate > 0 ? 1f / _animationData.FrameRate : 0.1f; // Default 10 FPS

        _elapsedTime += deltaTime;

        // Check if enough time has passed for the next frame
        while (_elapsedTime >= frameTime && IsPlaying)
        {
            _elapsedTime -= frameTime;
            _currentFrame++;

            if (_currentFrame >= _animationData.FrameTileIds.Length)
            {
                if (_animationData.Loop)
                {
                    _currentFrame = 0; // Loop back to start
                }
                else
                {
                    _currentFrame = _animationData.FrameTileIds.Length - 1;
                    IsComplete = true;
                    IsPlaying = false;
                    break;
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
        if (_animationData.FrameTileIds == null || _animationData.FrameTileIds.Length == 0)
        {
            return 0;
        }

        // Ensure we don't go out of bounds
        var frameIndex = Math.Max(0, Math.Min(_currentFrame, _animationData.FrameTileIds.Length - 1));
        return _animationData.FrameTileIds[frameIndex];
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
        // Only reset if this is actually a different animation
        var isDifferent = string.IsNullOrEmpty(_animationData.Name) ||
                          _animationData.Name != animationData.Name ||
                          _animationData.FrameTileIds?.Length != animationData.FrameTileIds?.Length;

        _animationData = animationData;

        if (isDifferent)
        {
            _elapsedTime = 0f;
            _currentFrame = 0;
            IsComplete = false;
        }

        IsPlaying = true;
    }
}
