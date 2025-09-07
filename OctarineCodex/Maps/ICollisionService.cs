using System.Collections.Generic;
using LDtk;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Maps;

/// <summary>
///     Collision detection service using LDtk IntGrid data.
/// </summary>
[Service<CollisionService>]
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