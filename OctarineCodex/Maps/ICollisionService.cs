using System;
using System.Collections.Generic;
using System.Linq;
using LDtk;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Maps;

/// <summary>
///     Collision detection service using LDtk IntGrid data.
/// </summary>
public interface ICollisionService
{
    /// <summary>
    ///     Initializes collision data for the specified levels.
    /// </summary>
    /// <param name="levels">Levels to process for collision</param>
    void InitializeCollision(IEnumerable<LDtkLevel> levels);

    /// <summary>
    ///     Checks if a rectangle collides with solid tiles.
    /// </summary>
    /// <param name="bounds">Rectangle to test</param>
    /// <returns>True if collision detected</returns>
    bool CheckCollision(RectangleF bounds);

    /// <summary>
    ///     Gets corrected position after collision resolution.
    /// </summary>
    /// <param name="currentPos">Current position</param>
    /// <param name="newPos">Desired new position</param>
    /// <param name="size">Entity size</param>
    /// <returns>Corrected position</returns>
    Vector2 ResolveCollision(Vector2 currentPos, Vector2 newPos, Vector2 size);
}

public class CollisionService : ICollisionService
{
    private readonly Dictionary<Point, bool> _solidTiles = new();
    private int _tileSize = 16;

    public void InitializeCollision(IEnumerable<LDtkLevel> levels)
    {
        _solidTiles.Clear();

        foreach (var level in levels)
        {
            if (level.LayerInstances == null) continue;

            // Find collision layer (typically IntGrid layer with identifier "Collision" or similar)
            var collisionLayer = level.LayerInstances
                .FirstOrDefault(l => l._Type == LayerType.IntGrid &&
                                     (l._Identifier.Contains("Collision", StringComparison.OrdinalIgnoreCase) ||
                                      l._Identifier.Contains("Solid", StringComparison.OrdinalIgnoreCase)));

            if (collisionLayer == null) continue;

            _tileSize = collisionLayer._GridSize;

            // Process IntGrid data
            for (var i = 0; i < collisionLayer.IntGridCsv.Length; i++)
                if (collisionLayer.IntGridCsv[i] > 0) // Non-zero means solid
                {
                    var tileX = level.WorldX + i % collisionLayer._CWid * _tileSize;
                    var tileY = level.WorldY + i / collisionLayer._CWid * _tileSize;
                    _solidTiles[new Point(tileX / _tileSize, tileY / _tileSize)] = true;
                }
        }
    }

    public bool CheckCollision(RectangleF bounds)
    {
        var startX = (int)(bounds.Left / _tileSize);
        var endX = (int)(bounds.Right / _tileSize);
        var startY = (int)(bounds.Top / _tileSize);
        var endY = (int)(bounds.Bottom / _tileSize);

        for (var x = startX; x <= endX; x++)
        for (var y = startY; y <= endY; y++)
            if (_solidTiles.TryGetValue(new Point(x, y), out var solid) && solid)
                return true;

        return false;
    }

    public Vector2 ResolveCollision(Vector2 currentPos, Vector2 newPos, Vector2 size)
    {
        var bounds = new RectangleF(newPos.X, newPos.Y, size.X, size.Y);

        if (!CheckCollision(bounds))
            return newPos;

        // Try X movement only
        var xOnlyBounds = new RectangleF(newPos.X, currentPos.Y, size.X, size.Y);
        if (!CheckCollision(xOnlyBounds))
            return new Vector2(newPos.X, currentPos.Y);

        // Try Y movement only  
        var yOnlyBounds = new RectangleF(currentPos.X, newPos.Y, size.X, size.Y);
        if (!CheckCollision(yOnlyBounds))
            return new Vector2(currentPos.X, newPos.Y);

        // No movement possible
        return currentPos;
    }
}

public struct RectangleF
{
    public float X, Y, Width, Height;

    public RectangleF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public float Left => X;
    public float Right => X + Width;
    public float Top => Y;
    public float Bottom => Y + Height;
}