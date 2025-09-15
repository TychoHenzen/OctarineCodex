namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Data for rendering a single animation layer.
/// </summary>
public readonly record struct LayerRenderData(
    string LayerName,
    int TileId,
    int Priority,
    float Alpha);
