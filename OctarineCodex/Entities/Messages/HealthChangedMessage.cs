namespace OctarineCodex.Entities.Messages;

public class HealthChangedMessage(int current, int max, int previousHealth = -1)
{
    public int Current { get; } = current;
    public int Max { get; } = max;
    public int PreviousHealth { get; } = previousHealth;
}
