using Microsoft.Xna.Framework;

namespace OctarineCodex;

/// <summary>
/// Pure helpers for movement direction and displacement calculations.
/// </summary>
public static class Movement
{
    /// <summary>
    /// Builds a normalized direction vector from WASD booleans.
    /// </summary>
    public static Vector2 GetDirection(bool up, bool down, bool left, bool right)
    {
        Vector2 dir = Vector2.Zero;
        if (up) dir.Y -= 1f;
        if (down) dir.Y += 1f;
        if (left) dir.X -= 1f;
        if (right) dir.X += 1f;
        if (dir != Vector2.Zero)
        {
            dir.Normalize();
        }
        return dir;
    }

    /// <summary>
    /// Computes displacement based on a direction vector, speed (pixels/sec), and delta time (sec).
    /// Direction will be normalized if not zero.
    /// </summary>
    public static Vector2 ComputeDelta(Vector2 direction, float speed, float dt)
    {
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
            return direction * speed * dt;
        }
        return Vector2.Zero;
    }
}