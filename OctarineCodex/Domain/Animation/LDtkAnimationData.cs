// Domain/Animation/LDtkAnimationData.cs

using System;
using System.Collections.Generic;
using OctarineCodex.Domain.Magic;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Animation data parsed from LDtk custom fields.
///     Supports both simple tile sequences and complex multi-state animations.
/// </summary>
public readonly record struct LDtkAnimationData(
    string Name,
    int[] FrameTileIds,
    float FrameRate,
    bool Loop = true,
    AnimationType Type = AnimationType.Simple,
    string? NextAnimation = null)
{
    /// <summary>
    ///     Creates a simple looping animation from consecutive tile IDs.
    /// </summary>
    public static LDtkAnimationData CreateSimple(string name, int startTileId, int frameCount, float frameRate)
    {
        var frames = new int[frameCount];
        for (var i = 0; i < frameCount; i++)
        {
            frames[i] = startTileId + i;
        }

        return new LDtkAnimationData(name, frames, frameRate);
    }

    /// <summary>
    ///     Creates animation from LDtk custom field array.
    /// </summary>
    public static LDtkAnimationData FromLDtkFields(
        string name,
        int[] frameTileIds,
        float frameRate,
        bool loop = true,
        string animationType = "Simple",
        string? nextAnimation = null)
    {
        var type = Enum.Parse<AnimationType>(animationType);

        return new LDtkAnimationData(name, frameTileIds, frameRate, loop, type, nextAnimation);
    }
}

/// <summary>
///     Defines how magic vectors influence animation behavior.
/// </summary>
public readonly record struct MagicInfluence(
    EleAspects.Element Element,
    float Multiplier,
    float MinEffect = 0.1f,
    float MaxEffect = 3.0f);

/// <summary>
///     Types of animations supported by the system.
/// </summary>
public enum AnimationType
{
    Simple, // Basic looping animation (torches, water)
    Triggered, // Event-driven animation (doors, spikes)
    StateMachine // Complex state-based animation (characters)
}

/// <summary>
///     Animation state definition for complex character animations.
/// </summary>
public readonly record struct AnimationState(
    string Name,
    LDtkAnimationData Animation,
    Dictionary<string, string> Transitions);
