using Microsoft.Xna.Framework;
using OctarineCodex.Domain.Physics;
using OctarineCodex.Domain.Physics.Shapes;

namespace OctarineCodex.Application.Components;

public sealed class CollisionComponent(ICollisionShape shape, CollisionLayers layers = CollisionLayers.Entity)
{
    public ICollisionShape Shape { get; set; } = shape;
    public CollisionLayers Layers { get; set; } = layers;
    public CollisionLayers CollidesWith { get; init; } = CollisionLayers.All;
    public bool IsTrigger { get; set; } = false;
    public bool IsStatic { get; set; } = false;
    public Vector2 Velocity { get; set; } = Vector2.Zero;

    public bool CanCollideWith(CollisionComponent other)
    {
        return (CollidesWith & other.Layers) != 0 && (other.CollidesWith & Layers) != 0;
    }
}
