// OctarineCodex/Domain/Animation/AsepriteAnimationLoader.cs

using System.Collections.Generic;
using System.Linq;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Loads and processes Aseprite animation data into game-ready animations
/// </summary>
public class AsepriteAnimationLoader
{
    /// <summary>
    ///     Convert Aseprite JSON data into structured animations using frame tags
    /// </summary>
    public Dictionary<string, AsepriteAnimation> LoadAnimations(AsepriteAnimationData asepriteData)
    {
        var animations = new Dictionary<string, AsepriteAnimation>();

        foreach (AsepriteFrameTag frameTag in asepriteData.Meta.FrameTags)
        {
            var animFrames = new List<AsepriteFrame>();

            // Extract frames by matching frame names that start with the animation name
            // Aseprite exports frames with names like "AnimationName.0", "AnimationName.1", etc.
            List<AsepriteFrame> matchingFrames = asepriteData.Frames
                .Where(kvp => kvp.Key.StartsWith(frameTag.Name + "."))
                .OrderBy(kvp =>
                {
                    // Extract the frame number from the key (e.g., "Walk_Right.2" -> 2)
                    var parts = kvp.Key.Split('.');
                    if (parts.Length > 1 && int.TryParse(parts[^1], out var frameNumber))
                    {
                        return frameNumber;
                    }

                    return 0;
                })
                .Select(kvp => kvp.Value)
                .ToList();

            // If no frames found by name matching, fall back to checking if we can use the from/to indices
            // This handles cases where frame names might not follow the expected pattern
            if (matchingFrames.Count == 0)
            {
                // This is a fallback for differently formatted Aseprite exports
                // but shouldn't be used for our current animation.json structure
                AsepriteFrame[] frameList = asepriteData.Frames.Values.ToArray();
                for (var i = frameTag.From; i <= frameTag.To && i < frameList.Length; i++)
                {
                    animFrames.Add(frameList[i]);
                }
            }
            else
            {
                animFrames = matchingFrames;
            }

            // Skip animations with no frames
            if (animFrames.Count == 0)
            {
                continue;
            }

            var loop = frameTag.Direction != "pingpong"; // Assume loop unless pingpong
            var animation = new AsepriteAnimation(
                frameTag.Name,
                animFrames,
                loop,
                frameTag.Direction);

            animations[frameTag.Name] = animation;
        }

        return animations;
    }
}
