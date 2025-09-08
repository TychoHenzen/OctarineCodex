namespace OctarineCodex.Entities.Messages;

public class HealMessage(int amount, string? healSource = null)
{
    public int Amount { get; } = amount;
    public string? HealSource { get; } = healSource;
}
