using Microsoft.Xna.Framework;

namespace OctarineCodex.Collisions;

public sealed class BoxShape : ICollisionShape
{
    public Rectangle Bounds { get; }
    public Vector2 Offset { get; }

    public BoxShape(Rectangle bounds, Vector2 offset = default)
    {
        Bounds = bounds;
        Offset = offset;
    }

    public override Rectangle GetBounds()
    {
        return new Rectangle((int)(Bounds.X + Offset.X), (int)(Bounds.Y + Offset.Y), Bounds.Width, Bounds.Height);
    }

    public override bool Intersects(ICollisionShape other)
    {
        return other switch
        {
            BoxShape box => GetBounds().Intersects(box.GetBounds()),
            CircleShape circle => CircleRectangleIntersection(circle, GetBounds()),
            CompositeShape composite => composite.Intersects(this),
            _ => false
        };
    }

    public override bool Contains(Vector2 point)
    {
        return GetBounds().Contains(point);
    }

    public override Vector2 GetClosestPoint(Vector2 point)
    {
        Rectangle bounds = GetBounds();
        return new Vector2(
            MathHelper.Clamp(point.X, bounds.Left, bounds.Right),
            MathHelper.Clamp(point.Y, bounds.Top, bounds.Bottom));
    }

    private static bool CircleRectangleIntersection(CircleShape circle, Rectangle rect)
    {
        var closestPoint = new Vector2(
            MathHelper.Clamp(circle.Center.X, rect.Left, rect.Right),
            MathHelper.Clamp(circle.Center.Y, rect.Top, rect.Bottom));

        return Vector2.DistanceSquared(circle.Center, closestPoint) <= circle.Radius * circle.Radius;
    }
}
