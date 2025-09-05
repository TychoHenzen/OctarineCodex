using System;
using Microsoft.Xna.Framework;
using OctarineCodex.Magic;

namespace OctarineCodex.Entities.Behaviors;

public abstract class EntityBehavior : IBehavior
{
    protected EntityWrapper Entity { get; private set; }
    protected IServiceProvider Services { get; private set; }

    /// <summary>
    ///     Called once when the behavior is attached to an entity
    /// </summary>
    public virtual void Initialize(EntityWrapper entity, IServiceProvider services)
    {
        Entity = entity;
        Services = services;
    }

    /// <summary>
    ///     Called every frame for active behaviors
    /// </summary>
    public virtual void Update(GameTime gameTime)
    {
    }

    /// <summary>
    ///     Handle messages sent to this entity
    /// </summary>
    public virtual void OnMessage<T>(T message) where T : class
    {
    }

    /// <summary>
    ///     Called when the behavior is removed from an entity
    /// </summary>
    public virtual void Cleanup()
    {
    }

    /// <summary>
    ///     Determines if this behavior should be applied to the given entity
    /// </summary>
    public abstract bool ShouldApplyTo(EntityWrapper entity);

    // Helper methods for common checks
    protected bool HasEntityType(string entityType)
    {
        return Entity.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase);
    }

    protected bool HasField(string fieldName)
    {
        return Entity.HasField(fieldName);
    }

    protected bool HasField<T>(string fieldName, Func<T, bool> condition = null)
    {
        return Entity.TryGetField<T>(fieldName, out var value) &&
               (condition == null || condition(value));
    }

    protected bool HasMagicVector(EleAspects.Element element, float threshold)
    {
        return Entity.TryGetField<Signature>("magicVector", out var vector) &&
               vector.GetComponent(element) > threshold;
    }
}