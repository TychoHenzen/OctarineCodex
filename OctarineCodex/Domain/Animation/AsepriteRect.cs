using System.Text.Json.Serialization;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Rectangle definition for frame positioning.
/// </summary>
public record AsepriteRect(
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("w")] int W,
    [property: JsonPropertyName("h")] int H);
