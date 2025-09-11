using Microsoft.Xna.Framework;

namespace OctarineCodex.Domain.Physics.Shapes;

public sealed class CircleShape(Vector2 center, float radius) : ICollisionShape
{
    public Vector2 Center => center;
    public float Radius => radius;

    public Rectangle GetFinalBounds()
    {
        return new Rectangle(
            (int)(Center.X - Radius),
            (int)(Center.Y - Radius),
            (int)(Radius * 2),
            (int)(Radius * 2));
    }

    public bool Intersects(ICollisionShape other)
    {
        return other switch
        {
            CircleShape circle => Vector2.DistanceSquared(Center, circle.Center) <=
                                  (Radius + circle.Radius) * (Radius + circle.Radius),
            BoxShape box => box.Intersects(this),
            CompositeShape composite => composite.Intersects(this),
            _ => false
        };
    }

    public bool Contains(Vector2 point)
    {
        return Vector2.DistanceSquared(Center, point) <= Radius * Radius;
    }

    public Vector2 GetClosestPoint(Vector2 point)
    {
        Vector2 direction = point - Center;
        if (direction.LengthSquared() > Radius * Radius)
        {
            direction.Normalize();
            return Center + (direction * Radius);
        }

        return point;
    }
}
