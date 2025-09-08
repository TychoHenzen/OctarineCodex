using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Extensions;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace OctarineCodex.Maps;

[UsedImplicitly]
public class CollisionService : ICollisionService
{
    private readonly Dictionary<Point, bool> _solidTiles = new();
    private int _tileSize = 16;

    public void InitializeCollision(IEnumerable<LDtkLevel> levels)
    {
        _solidTiles.Clear();

        foreach (var level in levels)
        {
            if (level.LayerInstances == null)
            {
                continue;
            }

            // Find collision layer (typically IntGrid layer with identifier "Collision" or similar)
            var collisionLayer = level.LayerInstances
                .FirstOrDefault(l => l._Type == LayerType.IntGrid &&
                                     (l._Identifier.Contains("Collision", StringComparison.OrdinalIgnoreCase) ||
                                      l._Identifier.Contains("Solid", StringComparison.OrdinalIgnoreCase)));

            if (collisionLayer == null)
            {
                continue;
            }

            _tileSize = collisionLayer._GridSize;

            // Process IntGrid data - Fixed coordinate calculation
            for (var i = 0; i < collisionLayer.IntGridCsv.Length; i++)
            {
                if (collisionLayer.IntGridCsv[i] <= 0)
                {
                    continue;
                }

                var gridX = i % collisionLayer._CWid;
                var gridY = i / collisionLayer._CWid;

                // Convert to world pixel coordinates
                var worldPixelX = level.WorldX + (gridX * _tileSize);
                var worldPixelY = level.WorldY + (gridY * _tileSize);

                // Convert to collision grid coordinates (handle negative coordinates properly)
                var collisionGridX = (int)Math.Floor((double)worldPixelX / _tileSize);
                var collisionGridY = (int)Math.Floor((double)worldPixelY / _tileSize);

                _solidTiles[new Point(collisionGridX, collisionGridY)] = true;
            }
        }
    }

    public bool CheckCollision(Rectangle bounds)
    {
        // Clean, functional approach using extension methods
        return bounds.ToTileCoordinates(_tileSize)
            .ForEachPosition(point =>
                _solidTiles.TryGetValue(point, out var solid)
                && solid);
    }

    public Vector2 ResolveCollision(Vector2 currentPos, Vector2 newPos, Vector2 size)
    {
        Rectangle bounds = GetTileBounds(newPos, size);

        if (!CheckCollision(bounds))
        {
            return newPos;
        }

        // Try X movement only
        Rectangle xOnlyBounds = GetTileBounds(new Vector2(newPos.X, currentPos.Y), size);
        if (!CheckCollision(xOnlyBounds))
        {
            return new Vector2(newPos.X, currentPos.Y);
        }

        // Try Y movement only
        Rectangle yOnlyBounds = GetTileBounds(new Vector2(currentPos.X, newPos.Y), size);
        if (!CheckCollision(yOnlyBounds))
        {
            return new Vector2(currentPos.X, newPos.Y);
        }

        // No movement possible
        return currentPos;
    }

    /// <summary>
    /// Converts floating-point entity bounds to integer tile coordinates using proper rounding.
    /// Uses floor for position and ceiling for size to ensure all potentially intersecting tiles are checked.
    /// </summary>
    private static Rectangle GetTileBounds(Vector2 position, Vector2 size)
    {
        var left = (int)Math.Floor(position.X);
        var top = (int)Math.Floor(position.Y);
        var right = (int)Math.Ceiling(position.X + size.X);
        var bottom = (int)Math.Ceiling(position.Y + size.Y);

        return new Rectangle(left, top, right - left, bottom - top);
    }
}
