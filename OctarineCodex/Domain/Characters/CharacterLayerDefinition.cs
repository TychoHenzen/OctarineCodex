// OctarineCodex/Domain/Characters/CharacterLayerDefinition.cs

using System.Collections.Generic;
using OctarineCodex.Domain.Animation;

namespace OctarineCodex.Domain.Characters;

/// <summary>
///     Updated to support both legacy SpriteSheetLayout and new Aseprite JSON format
/// </summary>
public record CharacterLayerDefinition(
    string LayerName,
    int Priority,
    string[] AvailableAssets,
    SpriteSheetLayout? Layout = null,
    string? AsepriteJsonPath = null)
{
    /// <summary>
    ///     True if this layer uses the new Aseprite JSON format
    /// </summary>
    public bool IsAsepriteFormat => !string.IsNullOrEmpty(AsepriteJsonPath);
}

public record CharacterAppearance(
    Dictionary<string, int> LayerSelections)
{
    public static CharacterAppearance Default => new(new Dictionary<string, int>
    {
        ["Body"] = 0,
        ["Eyes"] = 0,
        ["Outfit"] = 0,
        ["Hairstyle"] = 0,
        ["Accessory"] = 0
    });
}
