using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OctarineCodex;

/// <summary>
///     Represents the player entity with position, movement, and rendering capabilities.
/// </summary>
public class Player
{
    /// <summary>
    ///     The size of the player sprite in pixels.
    /// </summary>
    public const int Size = 8; // 1/4 of original 32 pixels

    /// <summary>
    ///     Initializes a new player at the specified position.
    /// </summary>
    /// <param name="initialPosition">The starting position of the player.</param>
    public Player(Vector2 initialPosition)
    {
        SetPosition(initialPosition);
    }

    /// <summary>
    ///     The current position of the player in world coordinates.
    /// </summary>
    public Vector2 Position { get; private set; }

    /// <summary>
    ///     Gets the player's bounding rectangle for collision detection.
    /// </summary>
    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, Size, Size);

    /// <summary>
    ///     Gets the center point of the player.
    /// </summary>
    public Vector2 Center => new(Position.X + Size / 2f, Position.Y + Size / 2f);

    /// <summary>
    ///     Updates the player's position based on movement delta, constrained by level bounds.
    /// </summary>
    /// <param name="movementDelta">The movement vector for this frame.</param>
    /// <param name="levelSize">The size of the level (width, height) for bounds checking.</param>
    public void Update(Vector2 movementDelta, Vector2 levelSize)
    {
        var newPosition = Position + movementDelta;

        // Clamp player position to level bounds
        newPosition.X = MathHelper.Clamp(newPosition.X, 0, levelSize.X - Size);
        newPosition.Y = MathHelper.Clamp(newPosition.Y, 0, levelSize.Y - Size);

        Position = newPosition;
    }

    /// <summary>
    ///     Sets the player's position directly, with bounds validation.
    /// </summary>
    /// <param name="position">The new position for the player.</param>
    /// <param name="levelSize">Optional level size for bounds checking. If null, no bounds checking is performed.</param>
    public void SetPosition(Vector2 position, Vector2? levelSize = null)
    {
        if (levelSize.HasValue)
        {
            position.X = MathHelper.Clamp(position.X, 0, levelSize.Value.X - Size);
            position.Y = MathHelper.Clamp(position.Y, 0, levelSize.Value.Y - Size);
        }

        Position = position;
    }

    /// <summary>
    ///     Renders the player using the provided sprite batch and texture.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to draw with (must be in Begin state).</param>
    /// <param name="texture">The texture to use for rendering the player.</param>
    /// <param name="color">The color tint to apply to the player.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D texture, Color color)
    {
        spriteBatch.Draw(texture,
            new Rectangle((int)Position.X, (int)Position.Y, Size, Size),
            color);
    }
}