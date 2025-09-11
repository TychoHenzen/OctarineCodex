using Microsoft.Xna.Framework;

namespace OctarineCodex.Messages;

public class MovementBlockedMessage(Vector2 intendedDirection, Vector2 intendedDelta, string? blockingReason = null)
{
    public Vector2 IntendedDirection { get; } = intendedDirection;
    public Vector2 IntendedDelta { get; } = intendedDelta;
    public string? BlockingReason { get; } = blockingReason;
}
