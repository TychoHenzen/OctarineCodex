// OctarineCodex/Entities/Messages/EntityMessages.cs

namespace OctarineCodex.Entities.Messages;

public class DamageMessage(int amount, string? damageSource = null)
{
    public int Amount { get; } = amount;
    public string? DamageSource { get; } = damageSource;
}
