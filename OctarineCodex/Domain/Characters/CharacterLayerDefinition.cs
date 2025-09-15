// OctarineCodex/Domain/Characters/CharacterLayerDefinition.cs

using System.Collections.Generic;

namespace OctarineCodex.Domain.Characters;

/// <summary>
///     Updated to support both legacy SpriteSheetLayout and new Aseprite JSON format.
/// </summary>
public record CharacterLayerDefinition(
    string LayerName,
    int Priority,
    List<string> AvailableAssets,
    string? JsonPath = null);
