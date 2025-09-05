// OctarineCodex/Entities/Messages/EntityMessages.cs

using Microsoft.Xna.Framework;

namespace OctarineCodex.Entities.Messages;

public class DamageMessage
{
    public DamageMessage(int amount)
    {
        Amount = amount;
    }

    public int Amount { get; }
}

public class HealMessage
{
    public HealMessage(int amount)
    {
        Amount = amount;
    }

    public int Amount { get; }
}

public class HealthChangedMessage
{
    public HealthChangedMessage(int current, int max)
    {
        Current = current;
        Max = max;
    }

    public int Current { get; }
    public int Max { get; }
}

public class EntityDeathMessage
{
}

public class InteractionMessage
{
    public InteractionMessage(EntityWrapper interactor)
    {
        Interactor = interactor;
    }

    public EntityWrapper Interactor { get; }
}

public class DoorOpenedMessage
{
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
    public MovementBlockedMessage(Vector2 intendedDirection, Vector2 intendedDelta)
    {
        IntendedDirection = intendedDirection;
        IntendedDelta = intendedDelta;
    }

    public Vector2 IntendedDirection { get; }
    public Vector2 IntendedDelta { get; }
}