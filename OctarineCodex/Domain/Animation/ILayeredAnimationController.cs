// Domain/Animation/ILayeredAnimationController.cs

using System.Collections.Generic;
using OctarineCodex.Application.Services;

namespace OctarineCodex.Domain.Animation;

/// <summary>
///     Interface for layered animation controllers that manage multiple synchronized animation layers.
/// </summary>
[Service<LayeredAnimationController>]
public interface ILayeredAnimationController : IAnimationComponent
{
    /// <summary>
    ///     Adds an animation layer (e.g., "Base", "Clothing", "Armor", "Weapon").
    /// </summary>
    void AddLayer(string layerName, Dictionary<string, LDtkAnimationData> animations, int priority = 0);

    /// <summary>
    ///     Removes an animation layer (e.g., when armor is removed).
    /// </summary>
    void RemoveLayer(string layerName);

    /// <summary>
    ///     Gets animation data for all layers, sorted by render priority.
    /// </summary>
    IEnumerable<LayerRenderData> GetLayerRenderData();
}
