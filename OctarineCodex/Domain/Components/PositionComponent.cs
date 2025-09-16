using System;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Domain.Components;

/// <summary>
///     Core position and transform component for ECS entities.
///     Contains spatial information required for rendering and physics.
/// </summary>
public struct PositionComponent
{
    /// <summary>
    ///     World position in pixels.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    ///     Rotation in radians.
    /// </summary>
    public float Rotation;

    /// <summary>
    ///     Scale factor for rendering. Default is (1, 1).
    /// </summary>
    public Vector2 Scale;

    /// <summary>
    ///     Creates a new position component with default values.
    /// </summary>
    /// <param name="position">Initial position</param>
    /// <param name="rotation">Initial rotation in radians (default: 0)</param>
    /// <param name="scale">Initial scale (default: 1,1)</param>
    public PositionComponent(Vector2 position, float rotation = 0f, Vector2 scale = default)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale == Vector2.Zero ? Vector2.One : scale;
    }

    /// <summary>
    ///     Creates a transformation matrix from this position component.
    /// </summary>
    public readonly Matrix ToMatrix()
    {
        return Matrix.CreateScale(Scale.X, Scale.Y, 1f) *
               Matrix.CreateRotationZ(Rotation) *
               Matrix.CreateTranslation(Position.X, Position.Y, 0f);
    }

    /// <summary>
    ///     Gets the forward direction vector based on current rotation.
    /// </summary>
    public readonly Vector2 Forward => new(MathF.Cos(Rotation), MathF.Sin(Rotation));

    /// <summary>
    ///     Gets the right direction vector based on current rotation.
    /// </summary>
    public readonly Vector2 Right => new(-MathF.Sin(Rotation), MathF.Cos(Rotation));
}
