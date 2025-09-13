// Domain/Animation/SpriteSheetLayout.cs

using System.Collections.Generic;
using System.Text.Json;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Defines the standard animation layout shared by all character spritesheets
/// </summary>
public record SpriteSheetLayout(
    string Name,
    int TileWidth,
    int TileHeight,
    int TilesPerRow,
    Dictionary<string, AnimationLayout> Animations)
{
    public static SpriteSheetLayout LoadFromJson(string jsonContent)
    {
        return JsonSerializer.Deserialize<SpriteSheetLayout>(jsonContent);
    }
}

public record AnimationLayout(
    string Name,
    int StartTileId,
    int FrameCount,
    float FrameRate,
    bool Loop = true
);
