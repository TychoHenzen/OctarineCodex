// Domain/Animation/LDtkAnimationData.cs

using System;

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
