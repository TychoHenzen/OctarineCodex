using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Domain.Physics.Shapes;

public sealed class CompositeShape(params ICollisionShape[] shapes) : ICollisionShape
{
    public IReadOnlyList<ICollisionShape> Shapes { get; } = shapes;

    public Rectangle GetFinalBounds()
    {
        if (Shapes.Count == 0)
        {
            return Rectangle.Empty;
        }

        Rectangle bounds = Shapes[0].GetFinalBounds();
        for (var i = 1; i < Shapes.Count; i++)
        {
            bounds = Rectangle.Union(bounds, Shapes[i].GetFinalBounds());
        }

        return bounds;
    }

    public bool Intersects(ICollisionShape other)
    {
        return Shapes.Any(shape => shape.Intersects(other));
    }

    public bool Contains(Vector2 point)
    {
        return Shapes.Any(shape => shape.Contains(point));
    }

    public Vector2 GetClosestPoint(Vector2 point)
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
            if (distance >= minDistance)
            {
                continue;
            }

            minDistance = distance;
            closestPoint = testPoint;
        }

        return closestPoint;
    }
}
