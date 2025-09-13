// OctarineCodex/Domain/Animation/AsepriteToLDtkBridge.cs

using System.Linq;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Bridges Aseprite animation data to existing LDtk animation system
///     Allows gradual migration and compatibility with existing magic system
/// </summary>
public class AsepriteToLDtkBridge
{
    /// <summary>
    ///     Convert Aseprite animation to LDtk format for compatibility
    /// </summary>
    public LDtkAnimationData ConvertToLDtkFormat(AsepriteAnimation asepriteAnim)
    {
        // Generate tile IDs based on frame positions
        // This assumes a standard tile layout
        var tileIds = asepriteAnim.Frames.Select(frame =>
            CalculateTileId(frame.Frame)).ToArray();

        return new LDtkAnimationData(
            asepriteAnim.Name,
            tileIds,
            asepriteAnim.FrameRate,
            asepriteAnim.Loop);
    }

    private static int CalculateTileId(AsepriteRect frame)
    {
        // Calculate tile ID based on frame position
        // Assumes 16x32 tiles in a standard layout
        const int tileWidth = 16;
        const int tileHeight = 32;
        const int tilesPerRow = 56; // Based on 896px width / 16px tiles

        var tileX = frame.X / tileWidth;
        var tileY = frame.Y / tileHeight;

        return (tileY * tilesPerRow) + tileX;
    }
}
