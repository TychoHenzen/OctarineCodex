using System.Text.Json.Serialization;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Metadata section containing animation tags.
/// </summary>
public record AsepriteMeta(
    [property: JsonPropertyName("app")] string App,
    [property: JsonPropertyName("version")]
    string Version,
    [property: JsonPropertyName("format")] string Format,
    [property: JsonPropertyName("size")] AsepriteSize Size,
    [property: JsonPropertyName("scale")] string Scale,
    [property: JsonPropertyName("frameTags")]
    AsepriteFrameTag[] FrameTags);
