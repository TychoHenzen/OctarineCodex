// OctarineCodex/Domain/Components/CollisionComponent.cs

using Microsoft.Xna.Framework;
using OctarineCodex.Domain.Physics;

namespace OctarineCodex.Domain.Components;

/// <summary>
///     ECS collision component for spatial interaction and physics.
///     Replaces the class-based Application.Components.CollisionComponent.
/// </summary>
public struct CollisionComponent
{
    public CollisionShapeType ShapeType;
    public Vector2 Size; // For Rectangle/Circle shapes
    public CollisionLayers Layers;
    public CollisionLayers CollidesWith;
    public bool IsTrigger;
    public bool IsStatic;
    public Vector2 Velocity;
    public Vector2 LastPosition; // For continuous collision detection

    public CollisionComponent(
        CollisionShapeType shapeType,
        Vector2 size,
        CollisionLayers layers = CollisionLayers.Entity)
    {
        ShapeType = shapeType;
        Size = size;
        Layers = layers;
        CollidesWith = CollisionLayers.All;
        IsTrigger = false;
        IsStatic = false;
        Velocity = Vector2.Zero;
        LastPosition = Vector2.Zero;
    }

    public readonly bool CanCollideWith(CollisionComponent other)
    {
        return (CollidesWith & other.Layers) != 0 && (other.CollidesWith & Layers) != 0;
    }
}

public enum CollisionShapeType { Rectangle, Circle, Point }
