// Presentation/Rendering/TileAnimationManager.cs

using System.Collections.Generic;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Domain.Animation;
using OctarineCodex.Infrastructure.LDtk;

namespace OctarineCodex.Presentation.Rendering;

/// <summary>
///     Manages animated tiles within the LevelRenderer system.
///     Integrates with LDtk animation definitions and supports magic system modifiers.
/// </summary>
public class TileAnimationManager
{
    private readonly Dictionary<int, SimpleAnimationComponent> _animatedTiles = [];
    private readonly Dictionary<int, LDtkAnimationData> _tileAnimationData = [];

    /// <summary>
    ///     Loads tile animations from LDtk tileset definitions.
    /// </summary>
    public void LoadTileAnimations(TilesetDefinition tileset)
    {
        Dictionary<int, LDtkAnimationData> animations = LDtkAnimationLoader.LoadTileAnimations(tileset);

        foreach (var (tileId, animData) in animations)
        {
            _tileAnimationData[tileId] = animData;

            var component = new SimpleAnimationComponent();
            component.SetAnimation(animData);
            _animatedTiles[tileId] = component;
        }
    }

    /// <summary>
    ///     Updates all animated tiles within the visible area.
    /// </summary>
    public void Update(GameTime gameTime, Rectangle visibleBounds)
    {
        // Only update animated tiles - static tiles remain unchanged
        foreach (SimpleAnimationComponent component in _animatedTiles.Values)
        {
            component.Update(gameTime);
        }
    }

    /// <summary>
    ///     Gets the current tile ID for an animated tile, or returns the original ID if not animated.
    /// </summary>
    public int GetCurrentTileId(int originalTileId)
    {
        if (_animatedTiles.TryGetValue(originalTileId, out SimpleAnimationComponent? component))
        {
            return component.GetCurrentTileId();
        }

        return originalTileId;
    }

    /// <summary>
    ///     Checks if a tile ID has animation data.
    /// </summary>
    public bool IsAnimatedTile(int tileId)
    {
        return _animatedTiles.ContainsKey(tileId);
    }
}
