using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Collisions;

public sealed class CompositeShape : ICollisionShape
{
    public IReadOnlyList<ICollisionShape> Shapes { get; }

    public CompositeShape(params ICollisionShape[] shapes)
    {
        Shapes = shapes;
    }

    public override Rectangle GetBounds()
    {
        if (Shapes.Count == 0)
        {
            return Rectangle.Empty;
        }

        Rectangle bounds = Shapes[0].GetBounds();
        for (var i = 1; i < Shapes.Count; i++)
        {
            bounds = Rectangle.Union(bounds, Shapes[i].GetBounds());
        }

        return bounds;
    }

    public override bool Intersects(ICollisionShape other)
    {
        return Shapes.Any(shape => shape.Intersects(other));
    }

    public override bool Contains(Vector2 point)
    {
        return Shapes.Any(shape => shape.Contains(point));
    }

    public override Vector2 GetClosestPoint(Vector2 point)
    {
        if (Shapes.Count == 0)
        {
            return point;
        }

        Vector2 closestPoint = Shapes[0].GetClosestPoint(point);
        var minDistance = Vector2.DistanceSquared(point, closestPoint);

        for (var i = 1; i < Shapes.Count; i++)
        {
            Vector2 testPoint = Shapes[i].GetClosestPoint(point);
            var distance = Vector2.DistanceSquared(point, testPoint);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = testPoint;
            }
        }

        return closestPoint;
    }
}
