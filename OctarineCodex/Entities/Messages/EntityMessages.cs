// OctarineCodex/Entities/Messages/EntityMessages.cs

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