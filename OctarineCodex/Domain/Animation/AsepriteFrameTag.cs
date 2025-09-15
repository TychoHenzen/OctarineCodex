using System.Text.Json.Serialization;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Animation tag definition from Aseprite.
/// </summary>
public record AsepriteFrameTag(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("from")] int From,
    [property: JsonPropertyName("to")] int To,
    [property: JsonPropertyName("direction")]
    string Direction,
    [property: JsonPropertyName("color")] string Color);
