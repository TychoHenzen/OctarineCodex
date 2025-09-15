using System.Text.Json.Serialization;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Size definition for source dimensions.
/// </summary>
public record AsepriteSize(
    [property: JsonPropertyName("w")] int W,
    [property: JsonPropertyName("h")] int H);
