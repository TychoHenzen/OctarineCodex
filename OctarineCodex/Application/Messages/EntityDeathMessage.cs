using Microsoft.Xna.Framework;

namespace OctarineCodex.Application.Messages;

public class EntityDeathMessage(Vector2? deathPosition = null, string? causeOfDeath = null)
{
    public Vector2? DeathPosition { get; } = deathPosition;
    public string? CauseOfDeath { get; } = causeOfDeath;
}
