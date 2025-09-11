// OctarineCodex/Entities/Behaviors/EntityBehavior.cs

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Application.Entities;
using OctarineCodex.Domain.Magic;

namespace OctarineCodex.Domain.Entities;

public abstract class EntityBehavior : IBehavior
{
    protected EntityWrapper Entity { get; private set; } = null!;

    /// <summary>
    ///     Called once when the behavior is attached to an entity.
    /// </summary>
    public virtual void Initialize(EntityWrapper entity)
    {
        Entity = entity;
    }

    /// <summary>
    ///     Called when the behavior is removed from an entity.
    /// </summary>
    public virtual void Cleanup()
    {
        // Default: do nothing
    }

    /// <summary>
    ///     Handle messages sent to this entity.
    /// </summary>
    public virtual void OnMessage<T>(T message)
        where T : class
    {
        // Default: do nothing
    }

    /// <summary>
    ///     Called every frame for active behaviors.
    /// </summary>
    public virtual void Update(GameTime gameTime)
    {
        // Default: do nothing
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        // Default: do nothing
    }

    /// <summary>
    ///     Determines if this behavior should be applied to the given entity.
    /// </summary>
    public abstract bool ShouldApplyTo(EntityWrapper entity);

    // Helper methods for common checks in ShouldApplyTo - these work with the passed entity
    protected static bool HasEntityType(EntityWrapper? entity, string entityType)
    {
        return entity?.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase) == true;
    }

    // Instance versions for use after Initialize() is called
    protected bool HasEntityType(string entityType)
    {
        return Entity.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase);
    }

    protected bool HasField(string fieldName)
    {
        return Entity.HasField(fieldName);
    }

    protected bool HasField<T>(string fieldName, Func<T?, bool>? condition = null)
    {
        return Entity.TryGetField(fieldName, out T? value) &&
               (condition == null || condition(value));
    }

    protected bool HasMagicVector(EleAspects.Element element, float threshold)
    {
        return Entity.TryGetField("magicVector", out Signature? vector) &&
               vector?.GetComponent(element) > threshold;
    }
}
