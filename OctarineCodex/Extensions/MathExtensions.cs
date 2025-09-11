using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace OctarineCodex.Extensions;

public static class MathExtensions
{
    public static bool EqualsAppr(this float lhs, float rhs, float epsilon = 0.001f)
    {
        return Math.Abs(lhs - rhs) <= epsilon;
    }

    public static bool EqualsAppr(this double lhs, double rhs, double epsilon = 0.001f)
    {
        return Math.Abs(lhs - rhs) <= epsilon;
    }
    /// <summary>
    /// Scales a rectangle's size by the specified factor while keeping the same position.
    /// </summary>
    /// <param name="rectangle">The rectangle to scale.</param>
    /// <param name="scale">The scaling factor.</param>
    /// <returns>A new rectangle with scaled dimensions.</returns>
    public static Rectangle Scale(this Rectangle rectangle, float scale)
    {
        return rectangle.Scale(scale, scale);
    }

    /// <summary>
    /// Scales a rectangle's size by the specified factors while keeping the same position.
    /// </summary>
    /// <param name="rectangle">The rectangle to scale.</param>
    /// <param name="scaleX">The X scaling factor.</param>
    /// <param name="scaleY">The Y scaling factor.</param>
    /// <returns>A new rectangle with scaled dimensions.</returns>
    public static Rectangle Scale(this Rectangle rectangle, float scaleX, float scaleY)
    {
        return new Rectangle(
            rectangle.X,
            rectangle.Y,
            (int)Math.Round(rectangle.Width * scaleX),
            (int)Math.Round(rectangle.Height * scaleY));
    }

    public static Rectangle Clone(this Rectangle rectangle)
    {
        return new Rectangle(
            rectangle.X,
            rectangle.Y,
            rectangle.Width,
            rectangle.Height);
    }
    /// <summary>
    /// Converts pixel coordinates to tile coordinates by dividing by tile size.
    /// </summary>
    /// <param name="rectangle">The rectangle in pixel coordinates.</param>
    /// <param name="tileSize">The size of each tile in pixels.</param>
    /// <returns>A new rectangle in tile coordinates.</returns>
    public static Rectangle ToTileCoordinates(this Rectangle rectangle, int tileSize)
    {
        var startX = rectangle.Left / tileSize;
        var endX = rectangle.Right / tileSize;
        var startY = rectangle.Top / tileSize;
        var endY = rectangle.Bottom / tileSize;

        return new Rectangle(startX, startY, endX - startX, endY - startY);
    }

    /// <summary>
    /// Executes an action for each position within the rectangle bounds (inclusive).
    /// </summary>
    /// <param name="rectangle">The rectangle to iterate over.</param>
    /// <param name="action">The action to execute for each position.</param>
    public static void ForEachPosition(this Rectangle rectangle, Action<Point>? action)
    {
        if (action == null)
        {
            return;
        }

        foreach (Point p in rectangle.GetPositions())
        {
            action(p);
        }
    }

    /// <summary>
    /// Executes an action for each position within the rectangle bounds (inclusive).
    /// Provides early exit capability by returning false from the action.
    /// </summary>
    /// <param name="rectangle">The rectangle to iterate over.</param>
    /// <param name="condition">The action to execute for each position. Return false to stop iteration.</param>
    /// <returns>True if all positions were processed, false if iteration was stopped early.</returns>
    public static bool ForEachPosition(this Rectangle rectangle, Func<Point, bool>? condition)
    {
        return condition != null && rectangle.GetPositions().All(condition);
    }

    /// <summary>
    /// Gets all integer positions within the rectangle bounds (inclusive).
    /// </summary>
    /// <param name="rectangle">The rectangle to iterate over.</param>
    /// <returns>An enumerable of all points within the rectangle.</returns>
    private static IEnumerable<Point> GetPositions(this Rectangle rectangle)
    {
        for (var x = rectangle.Left; x <= rectangle.Right; x++)
        {
            for (var y = rectangle.Top; y <= rectangle.Bottom; y++)
            {
                yield return new Point(x, y);
            }
        }
    }
}
