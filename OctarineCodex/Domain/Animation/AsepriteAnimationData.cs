// OctarineCodex/Domain/Animation/AsepriteAnimationData.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Root structure for Aseprite JSON animation data
/// </summary>
public record AsepriteAnimationData(
    [property: JsonPropertyName("frames")] Dictionary<string, AsepriteFrame> Frames,
    [property: JsonPropertyName("meta")] AsepriteMeta Meta)
{
    public static AsepriteAnimationData FromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<AsepriteAnimationData>(json, options)
               ?? throw new InvalidOperationException("Failed to parse Aseprite JSON data");
    }
}

/// <summary>
///     Individual frame data from Aseprite JSON
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

/// <summary>
///     Rectangle definition for frame positioning
/// </summary>
public record AsepriteRect(
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("w")] int W,
    [property: JsonPropertyName("h")] int H);

/// <summary>
///     Size definition for source dimensions
/// </summary>
public record AsepriteSize(
    [property: JsonPropertyName("w")] int W,
    [property: JsonPropertyName("h")] int H);

/// <summary>
///     Metadata section containing animation tags
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

/// <summary>
///     Animation tag definition from Aseprite
/// </summary>
public record AsepriteFrameTag(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("from")] int From,
    [property: JsonPropertyName("to")] int To,
    [property: JsonPropertyName("direction")]
    string Direction,
    [property: JsonPropertyName("color")] string Color);

/// <summary>
///     Processed animation data ready for game use
/// </summary>
public record AsepriteAnimation(
    string Name,
    List<AsepriteFrame> Frames,
    bool Loop = true,
    string Direction = "forward")
{
    /// <summary>
    ///     Calculate average frame duration in milliseconds
    /// </summary>
    public float AverageFrameDuration =>
        (float)(Frames.Count > 0 ? Frames.Average(f => f.Duration) : 100f);

    /// <summary>
    ///     Calculate frames per second from average duration
    /// </summary>
    public float FrameRate => 1000f / AverageFrameDuration;
}
