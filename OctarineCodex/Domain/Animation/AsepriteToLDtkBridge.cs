// OctarineCodex/Domain/Animation/AsepriteToLDtkBridge.cs

using System.Linq;

namespace OctarineCodex.Domain.Animation;

/// <summary>
/// Bridges Aseprite animation data to existing LDtk animation system
/// Allows gradual migration and compatibility with existing magic system.
/// </summary>
public static class AsepriteToLDtkBridge
{
    /// <summary>
    /// Convert Aseprite animation to LDtk format for compatibility.
    /// </summary>
    public static LDtkAnimationData ConvertToLDtkFormat(AsepriteAnimation asepriteAnim)
    {
        // Generate tile IDs based on frame positions
        var tileIds = asepriteAnim.Frames.Select(frame =>
            CalculateTileId(frame.Frame)).ToArray();

        // Use a reasonable default frame rate if not specified
        var frameRate = asepriteAnim.FrameRate > 0 ? asepriteAnim.FrameRate : 10f;

        return new LDtkAnimationData(
            asepriteAnim.Name,
            tileIds,
            frameRate,
            asepriteAnim.Loop);
    }

    private static int CalculateTileId(AsepriteRect frame)
    {
        // Calculate tile ID based on frame position
        // Based on animation.json: 16x32 tiles in 896x640 sprite sheet
        const int tileWidth = 16;
        const int tileHeight = 32;
        const int tilesPerRow = 56; // 896px width / 16px tiles = 56

        var tileX = frame.X / tileWidth;
        var tileY = frame.Y / tileHeight;

        return (tileY * tilesPerRow) + tileX;
    }
}
