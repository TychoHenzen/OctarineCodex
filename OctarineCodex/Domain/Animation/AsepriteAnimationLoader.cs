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

        // Convert frame dictionary to indexed list for frame tag processing
        AsepriteFrame[] frameList = asepriteData.Frames.Values.ToArray();

        foreach (AsepriteFrameTag frameTag in asepriteData.Meta.FrameTags)
        {
            var animFrames = new List<AsepriteFrame>();

            // Extract frames for this animation based on frame tag indices
            for (var i = frameTag.From; i <= frameTag.To && i < frameList.Length; i++)
            {
                animFrames.Add(frameList[i]);
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
