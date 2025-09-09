using Microsoft.Xna.Framework;

namespace OctarineCodex.Collisions;

public sealed class CircleShape : ICollisionShape
{
    public Vector2 Center { get; }
    public float Radius { get; }

    public CircleShape(Vector2 center, float radius)
    {
        Center = center;
        Radius = radius;
    }

    public override Rectangle GetBounds()
    {
        return new Rectangle((int)(Center.X - Radius), (int)(Center.Y - Radius),
            (int)(Radius * 2), (int)(Radius * 2));
    }

    public override bool Intersects(ICollisionShape other)
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

    public override bool Contains(Vector2 point)
    {
        return Vector2.DistanceSquared(Center, point) <= Radius * Radius;
    }

    public override Vector2 GetClosestPoint(Vector2 point)
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
