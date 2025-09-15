using System.Collections.Generic;
using System.Linq;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Processed animation data ready for game use.
/// </summary>
public record AsepriteAnimation(
    string Name,
    List<AsepriteFrame> Frames,
    bool Loop = true,
    string Direction = "forward")
{
    /// <summary>
    ///     Gets average frame duration in milliseconds.
    /// </summary>
    public float AverageFrameDuration =>
        (float)(Frames.Count > 0 ? Frames.Average(f => f.Duration) : 100f);

    /// <summary>
    ///     Gets frames per second from average duration.
    /// </summary>
    public float FrameRate => 1000f / AverageFrameDuration;
}
