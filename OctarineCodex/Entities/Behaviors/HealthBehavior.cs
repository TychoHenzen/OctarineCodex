// OctarineCodex/Entities/Behaviors/HealthBehavior.cs

using System;
using Microsoft.Xna.Framework;
using OctarineCodex.Messages;

namespace OctarineCodex.Entities.Behaviors;

[EntityBehavior(RequiredFields = ["life"], Priority = 100)]
public class HealthBehavior : EntityBehavior
{
    private int _currentHealth;
    private int _maxHealth;

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        // Apply to any entity with a "life" field that's greater than 0
        return entity.TryGetField<int>("life", out var life) && life > 0;
    }

    public override void Initialize(EntityWrapper entity)
    {
        base.Initialize(entity);
        _maxHealth = Entity.GetField<int>("life");
        _currentHealth = _maxHealth;
    }

    public override void Update(GameTime gameTime)
    {
        // Health regeneration, status effects, etc.
    }

    public override void OnMessage<T>(T message)
    {
        switch (message)
        {
            case DamageMessage damage:
                TakeDamage(damage.Amount, damage.DamageSource);
                break;
            case HealMessage heal:
                Heal(heal.Amount);
                break;
        }
    }

    private void TakeDamage(int amount, string? damageSource = null)
    {
        var previousHealth = _currentHealth;
        _currentHealth = Math.Max(0, _currentHealth - amount);

        // Send health changed message locally for UI updates
        Entity.SendMessage(new HealthChangedMessage(_currentHealth, _maxHealth, previousHealth));

        // Send death message globally so other systems can react (loot, scoring, etc.)
        if (_currentHealth <= 0)
        {
            Entity.SendGlobalMessage(new EntityDeathMessage(Entity.Position, damageSource));
        }
    }

    private void Heal(int amount)
    {
        var previousHealth = _currentHealth;
        _currentHealth = Math.Min(_maxHealth, _currentHealth + amount);
        Entity.SendMessage(new HealthChangedMessage(_currentHealth, _maxHealth, previousHealth));
    }
}
