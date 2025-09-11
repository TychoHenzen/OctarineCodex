using Microsoft.Xna.Framework;
using OctarineCodex.Extensions;

namespace OctarineCodex.Domain.Physics.Shapes;

public sealed class BoxShape(Rectangle bounds, Vector2 offset = default) : ICollisionShape
{
    public Rectangle Bounds => bounds;
    public Vector2 Offset => offset;

    public Rectangle GetFinalBounds()
    {
        Rectangle ret = bounds.Clone();
        ret.Offset(offset);
        return ret;
    }

    public bool Intersects(ICollisionShape other)
    {
        return other switch
        {
            BoxShape box => GetFinalBounds().Intersects(box.GetFinalBounds()),
            CircleShape circle => CircleRectangleIntersection(circle, GetFinalBounds()),
            CompositeShape composite => composite.Intersects(this),
            _ => false
        };
    }

    public bool Contains(Vector2 point)
    {
        return GetFinalBounds().Contains(point);
    }

    public Vector2 GetClosestPoint(Vector2 point)
    {
        Rectangle final = GetFinalBounds();
        return new Vector2(
            MathHelper.Clamp(point.X, final.Left, final.Right),
            MathHelper.Clamp(point.Y, final.Top, final.Bottom));
    }

    private static bool CircleRectangleIntersection(CircleShape circle, Rectangle rect)
    {
        var closestPoint = new Vector2(
            MathHelper.Clamp(circle.Center.X, rect.Left, rect.Right),
            MathHelper.Clamp(circle.Center.Y, rect.Top, rect.Bottom));

        return Vector2.DistanceSquared(circle.Center, closestPoint) <= circle.Radius * circle.Radius;
    }
}
