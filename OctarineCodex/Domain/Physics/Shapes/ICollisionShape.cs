using Microsoft.Xna.Framework;

namespace OctarineCodex.Domain.Physics.Shapes;

public interface ICollisionShape
{
    public abstract Rectangle GetFinalBounds();
    public abstract bool Intersects(ICollisionShape other);
    public abstract bool Contains(Vector2 point);
    public abstract Vector2 GetClosestPoint(Vector2 point);
}
