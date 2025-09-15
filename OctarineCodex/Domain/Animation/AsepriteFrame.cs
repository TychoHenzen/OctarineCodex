using System.Text.Json.Serialization;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Individual frame data from Aseprite JSON.
/// </summary>
public record AsepriteFrame(
    [property: JsonPropertyName("frame")] AsepriteRect Frame,
    [property: JsonPropertyName("rotated")]
    bool Rotated,
    [property: JsonPropertyName("trimmed")]
    bool Trimmed,
    [property: JsonPropertyName("spriteSourceSize")]
    AsepriteRect SpriteSourceSize,
    [property: JsonPropertyName("sourceSize")]
    AsepriteSize SourceSize,
    [property: JsonPropertyName("duration")]
    int Duration);
