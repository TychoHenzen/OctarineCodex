using Microsoft.Xna.Framework;

namespace OctarineCodex.Entities.Messages;

public class PlayerMovedMessage(Vector2 newPosition, Vector2 delta)
{
    public Vector2 NewPosition { get; } = newPosition;
    public Vector2 Delta { get; } = delta;
}
