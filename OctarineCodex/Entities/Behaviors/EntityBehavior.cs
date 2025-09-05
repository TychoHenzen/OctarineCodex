// OctarineCodex/Entities/Behaviors/EntityBehavior.cs

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        // Default: do nothing
    }

    /// <summary>
    ///     Determines if this behavior should be applied to the given entity
    /// </summary>
    public abstract bool ShouldApplyTo(EntityWrapper entity);

    // Helper methods for common checks in ShouldApplyTo - these work with the passed entity
    protected static bool HasEntityType(EntityWrapper entity, string entityType)
    {
        return entity.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase);
    }

    protected static bool HasField(EntityWrapper entity, string fieldName)
    {
        return entity.HasField(fieldName);
    }

    protected static bool HasField<T>(EntityWrapper entity, string fieldName, Func<T, bool> condition = null)
    {
        return entity.TryGetField<T>(fieldName, out var value) &&
               (condition == null || condition(value));
    }

    protected static bool HasMagicVector(EntityWrapper entity, EleAspects.Element element, float threshold)
    {
        return entity.TryGetField<Signature>("magicVector", out var vector) &&
               vector.GetComponent(element) > threshold;
    }

    // Instance versions for use after Initialize() is called
    protected bool HasEntityType(string entityType)
    {
        return Entity?.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    protected bool HasField(string fieldName)
    {
        return Entity?.HasField(fieldName) ?? false;
    }

    protected bool HasField<T>(string fieldName, Func<T, bool> condition = null)
    {
        return Entity?.TryGetField<T>(fieldName, out var value) == true &&
               (condition == null || condition(value));
    }

    protected bool HasMagicVector(EleAspects.Element element, float threshold)
    {
        return Entity?.TryGetField<Signature>("magicVector", out var vector) == true &&
               vector.GetComponent(element) > threshold;
    }
}