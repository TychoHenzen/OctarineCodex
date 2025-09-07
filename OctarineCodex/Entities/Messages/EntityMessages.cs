// OctarineCodex/Entities/Messages/EntityMessages.cs

using Microsoft.Xna.Framework;

namespace OctarineCodex.Entities.Messages;

public class DamageMessage
{
    public DamageMessage(int amount, string? damageSource = null)
    {
        Amount = amount;
        DamageSource = damageSource;
    }

    public int Amount { get; }
    public string? DamageSource { get; }
}

public class HealMessage
{
    public HealMessage(int amount, string? healSource = null)
    {
        Amount = amount;
        HealSource = healSource;
    }

    public int Amount { get; }
    public string? HealSource { get; }
}

public class HealthChangedMessage
{
    public HealthChangedMessage(int current, int max, int previousHealth = -1)
    {
        Current = current;
        Max = max;
        PreviousHealth = previousHealth;
    }

    public int Current { get; }
    public int Max { get; }
    public int PreviousHealth { get; }
}

public class EntityDeathMessage
{
    public EntityDeathMessage(Vector2? deathPosition = null, string? causeOfDeath = null)
    {
        DeathPosition = deathPosition;
        CauseOfDeath = causeOfDeath;
    }

    public Vector2? DeathPosition { get; }
    public string? CauseOfDeath { get; }
}

public class InteractionMessage
{
    public InteractionMessage(EntityWrapper interactor, string? interactionType = null)
    {
        Interactor = interactor;
        InteractionType = interactionType;
    }

    public EntityWrapper Interactor { get; }
    public string? InteractionType { get; }
}

public class DoorOpenedMessage
{
    public DoorOpenedMessage(Vector2? doorPosition = null, string? doorId = null)
    {
        DoorPosition = doorPosition;
        DoorId = doorId;
    }

    public Vector2? DoorPosition { get; }
    public string? DoorId { get; }
}

public class PlayerMovedMessage
{
    public PlayerMovedMessage(Vector2 newPosition, Vector2 delta)
    {
        NewPosition = newPosition;
        Delta = delta;
    }

    public Vector2 NewPosition { get; }
    public Vector2 Delta { get; }
}

public class MovementBlockedMessage
{
    public MovementBlockedMessage(Vector2 intendedDirection, Vector2 intendedDelta, string? blockingReason = null)
    {
        IntendedDirection = intendedDirection;
        IntendedDelta = intendedDelta;
        BlockingReason = blockingReason;
    }

    public Vector2 IntendedDirection { get; }
    public Vector2 IntendedDelta { get; }
    public string? BlockingReason { get; }
}