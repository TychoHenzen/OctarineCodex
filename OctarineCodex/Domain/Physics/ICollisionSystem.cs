using System.Collections.Generic;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Application.Components;
using OctarineCodex.Application.Services;
using OctarineCodex.Domain.Physics.Shapes;
using OctarineCodex.Infrastructure.Physics;

namespace OctarineCodex.Domain.Physics;

[Service<CollisionSystem>]
public interface ICollisionSystem
{
    /// <summary>
    /// Initialize collision data from LDTK levels.
    /// </summary>
    void InitializeLevels(IEnumerable<LDtkLevel> levels);

    /// <summary>
    /// Register an entity with collision component.
    /// </summary>
    void RegisterEntity(string entityId, CollisionComponent component, Vector2 position);

    /// <summary>
    /// Unregister an entity from collision system.
    /// </summary>
    void UnregisterEntity(string entityId);

    /// <summary>
    /// Update entity position and check for collisions.
    /// </summary>
    void UpdateEntityPosition(string entityId, Vector2 newPosition);

    /// <summary>
    /// Test a line segment for collision.
    /// </summary>
    CollisionTestResult Linecast(Vector2 start, Vector2 end, CollisionLayers layersMask = CollisionLayers.All);

    /// <summary>
    /// Test a ray for collision.
    /// </summary>
    CollisionTestResult Raycast(Vector2 origin, Vector2 direction, float maxDistance = float.MaxValue,
        CollisionLayers layersMask = CollisionLayers.All);

    /// <summary>
    /// Test a shape at a position for overlaps.
    /// </summary>
    IEnumerable<CollisionTestResult> OverlapShape(ICollisionShape shape, Vector2 position,
        CollisionLayers layersMask = CollisionLayers.All);

    /// <summary>
    /// Sweep a shape along a path and test for collision.
    /// </summary>
    CollisionTestResult ShapeCast(ICollisionShape shape, Vector2 start, Vector2 end,
        CollisionLayers layersMask = CollisionLayers.All);

    /// <summary>
    /// Resolve movement collision for an entity.
    /// </summary>
    Vector2 ResolveMovement(string entityId, Vector2 currentPos, Vector2 desiredPos);

    /// <summary>
    /// Process collisions and send messages (called once per frame).
    /// </summary>
    void ProcessCollisions();

    /// <summary>
    /// Clear all collision data.
    /// </summary>
    void Clear();
}
