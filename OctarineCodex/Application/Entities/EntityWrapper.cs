// OctarineCodex/Application/Entities/EntityWrapper.cs

using System;
using System.Collections.Generic;
using System.Reflection;
using DefaultEcs;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Application.Messaging;
using OctarineCodex.Domain.Components;
using OctarineCodex.Domain.Entities;

namespace OctarineCodex.Application.Entities;

/// <summary>
///     Wrapper around LDtk entities that provides behavior management and component system.
///     Bridge between LDtk data and game runtime during Phase 2 migration.
/// </summary>
public class EntityWrapper : IDisposable
{
    private readonly List<IBehavior> _behaviors = [];
    private readonly IMessageBus _messageBus;
    private readonly ILDtkEntity _underlyingEntity;
    private bool _disposed;

    // Bridge properties for ECS integration
    private Entity? _ecsEntity;

    public EntityWrapper(ILDtkEntity underlyingEntity, IMessageBus messageBus)
    {
        _underlyingEntity = underlyingEntity ?? throw new ArgumentNullException(nameof(underlyingEntity));
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
    }

    /// <summary>
    /// Gets the underlying LDtk entity.
    /// </summary>
    public ILDtkEntity UnderlyingEntity => _underlyingEntity;

    /// <summary>
    ///     Gets the entity type identifier.
    /// </summary>
    public string EntityType => _underlyingEntity.Identifier;

    /// <summary>
    ///     Gets or sets the entity position.
    /// </summary>
    public Vector2 Position
    {
        get => _underlyingEntity.Position;
        set => _underlyingEntity.Position = value;
    }

    /// <summary>
    ///     Gets the entity size.
    /// </summary>
    public Vector2 Size => _underlyingEntity.Size;

    /// <summary>
    /// Gets the entity's unique identifier.
    /// </summary>
    public Guid Iid => _underlyingEntity.Iid;

    /// <summary>
    /// Gets the entity's UID.
    /// </summary>
    public int Uid => _underlyingEntity.Uid;

    /// <summary>
    /// Gets the associated ECS entity if this EntityWrapper has been bridged.
    /// </summary>
    public Entity? EcsEntity => _ecsEntity;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (IBehavior behavior in _behaviors)
        {
            if (behavior is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _behaviors.Clear();

        _ecsEntity?.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Adds a behavior to this entity.
    /// </summary>
    public void AddBehavior(IBehavior behavior)
    {
        if (behavior == null)
        {
            throw new ArgumentNullException(nameof(behavior));
        }

        _behaviors.Add(behavior);
        if (behavior is EntityBehavior entityBehavior)
        {
            entityBehavior.Initialize(this);
        }
    }

    /// <summary>
    /// Gets a behavior of the specified type.
    /// </summary>
    public T? GetBehavior<T>() where T : class, IBehavior
    {
        foreach (IBehavior behavior in _behaviors)
        {
            if (behavior is T typedBehavior)
            {
                return typedBehavior;
            }
        }

        return null;
    }

    /// <summary>
    /// Updates all behaviors.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        foreach (IBehavior behavior in _behaviors)
        {
            behavior.Update(gameTime);
        }

        // Sync to ECS entity if bridged
        SyncToEcsEntity();
    }

    /// <summary>
    /// Draws all behaviors.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (IBehavior behavior in _behaviors)
        {
            behavior.Draw(spriteBatch);
        }
    }

    // Component system methods (legacy EntityWrapper approach)
    public bool HasComponent<T>() where T : class
    {
        // For Phase 2, we'll use the ECS entity if available
        if (_ecsEntity.HasValue && _ecsEntity.Value.Has<T>())
        {
            return true;
        }

        // Legacy component check - simplified for Phase 2
        return false;
    }

    public T? GetComponent<T>() where T : class, new()
    {
        // For Phase 2, use ECS entity if available
        if (_ecsEntity.HasValue && _ecsEntity.Value.Has<T>())
        {
            return _ecsEntity.Value.Get<T>();
        }

        // Legacy component retrieval - simplified for Phase 2
        return new T();
    }

    // Field access methods for LDtk custom fields
    public bool HasField(string fieldName)
    {
        PropertyInfo? property = _underlyingEntity.GetType().GetProperty(fieldName);
        return property != null;
    }

    public bool TryGetField<T>(string fieldName, out T? value)
    {
        value = default(T);
        PropertyInfo? property = _underlyingEntity.GetType().GetProperty(fieldName);
        if (property == null)
        {
            return false;
        }

        var rawValue = property.GetValue(_underlyingEntity);
        if (rawValue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        return false;
    }

    public T? GetField<T>(string fieldName)
    {
        TryGetField(fieldName, out T? value);
        return value;
    }

    // Messaging methods
    public void SendMessage<T>(T message, string? targetEntityId = null) where T : class
    {
        var options = new MessageOptions
        {
            Scope = targetEntityId != null ? MessageScope.Entity : MessageScope.Global,
            TargetEntityId = targetEntityId
        };
        _messageBus.SendMessage(message, options, Iid.ToString());
    }

    public void SendGlobalMessage<T>(T message) where T : class
    {
        var options = new MessageOptions { Scope = MessageScope.Global };
        _messageBus.SendMessage(message, options, Iid.ToString());
    }

    /// <summary>
    ///     Bridges this EntityWrapper to an ECS entity for dual-system operation.
    /// </summary>
    internal void BridgeToEcsEntity(Entity ecsEntity)
    {
        _ecsEntity = ecsEntity;

        // Sync initial position to ECS
        if (_ecsEntity.Value.Has<PositionComponent>())
        {
            ref PositionComponent ecsPos = ref _ecsEntity.Value.Get<PositionComponent>();
            ecsPos.Position = Position;
        }
        else
        {
            _ecsEntity.Value.Set(new PositionComponent(Position));
        }
    }

    /// <summary>
    /// Syncs changes from EntityWrapper to ECS entity.
    /// Called during migration phase to keep systems in sync.
    /// </summary>
    public void SyncToEcsEntity()
    {
        if (!_ecsEntity.HasValue)
        {
            return;
        }

        // Sync position changes
        if (_ecsEntity.Value.Has<PositionComponent>())
        {
            ref PositionComponent ecsPos = ref _ecsEntity.Value.Get<PositionComponent>();
            ecsPos.Position = Position;
        }
    }
}
