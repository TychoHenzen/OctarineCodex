// OctarineCodex/Domain/Animation/AsepriteAnimationData.cs

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Root structure for Aseprite JSON animation data.
/// </summary>
public record AsepriteAnimationData(
    [property: JsonPropertyName("frames")] Dictionary<string, AsepriteFrame> Frames,
    [property: JsonPropertyName("meta")] AsepriteMeta Meta)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true
    };

    public static AsepriteAnimationData FromJson(string json)
    {
        return JsonSerializer.Deserialize<AsepriteAnimationData>(json, Options)
               ?? throw new InvalidOperationException("Failed to parse Aseprite JSON data");
    }
}
