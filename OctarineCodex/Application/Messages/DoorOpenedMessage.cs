using Microsoft.Xna.Framework;

namespace OctarineCodex.Application.Messages;

public class DoorOpenedMessage(Vector2? doorPosition = null, string? doorId = null)
{
    public Vector2? DoorPosition { get; } = doorPosition;
    public string? DoorId { get; } = doorId;
}
