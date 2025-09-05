using System;
using Microsoft.Xna.Framework;
using OctarineCodex.Entities.Messages;

namespace OctarineCodex.Entities.Behaviors;

[EntityBehavior(RequiredFields = ["life"], Priority = 100)]
public class HealthBehavior : EntityBehavior
{
    private int _currentHealth;
    private int _maxHealth;

    public override bool ShouldApplyTo(EntityWrapper entity)
    {
        // Apply to any entity with a "life" field that's greater than 0
        return HasField<int>("life", life => life > 0);
    }

    public override void Initialize(EntityWrapper entity, IServiceProvider services)
    {
        base.Initialize(entity, services);
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
                TakeDamage(damage.Amount);
                break;
            case HealMessage heal:
                Heal(heal.Amount);
                break;
        }
    }

    private void TakeDamage(int amount)
    {
        _currentHealth = Math.Max(0, _currentHealth - amount);
        Entity.SendMessage(new HealthChangedMessage(_currentHealth, _maxHealth));

        if (_currentHealth <= 0) Entity.SendMessage(new EntityDeathMessage());
    }

    private void Heal(int amount)
    {
        _currentHealth = Math.Min(_maxHealth, _currentHealth + amount);
        Entity.SendMessage(new HealthChangedMessage(_currentHealth, _maxHealth));
    }
}