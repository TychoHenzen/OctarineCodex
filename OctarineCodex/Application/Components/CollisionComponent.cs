using Microsoft.Xna.Framework;
using OctarineCodex.Domain.Physics;
using OctarineCodex.Domain.Physics.Shapes;

namespace OctarineCodex.Application.Components;

public sealed class CollisionComponent
{
    public ICollisionShape Shape { get; set; }
    public CollisionLayers Layers { get; set; }
    public CollisionLayers CollidesWith { get; set; }
    public bool IsTrigger { get; set; }
    public bool IsStatic { get; set; }
    public Vector2 Velocity { get; set; }

    public CollisionComponent(ICollisionShape shape, CollisionLayers layers = CollisionLayers.Entity)
    {
        Shape = shape;
        Layers = layers;
        CollidesWith = CollisionLayers.All;
        IsTrigger = false;
        IsStatic = false;
        Velocity = Vector2.Zero;
    }

    public bool CanCollideWith(CollisionComponent other)
    {
        return (CollidesWith & other.Layers) != 0 && (other.CollidesWith & Layers) != 0;
    }
}
