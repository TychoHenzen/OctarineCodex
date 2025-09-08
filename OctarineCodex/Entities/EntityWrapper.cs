using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Entities.Behaviors;
using OctarineCodex.Messaging;

namespace OctarineCodex.Entities;

public sealed class EntityWrapper : IMessageReceiver, IDisposable
{
    // Static instances to reduce allocations
    private static readonly MessageOptions LocalImmediateOptions =
        new() { Scope = MessageScope.Local, Immediate = true };

    private readonly List<IBehavior> _behaviors = [];
    private readonly Dictionary<Type, IBehavior> _behaviorsByType = [];
    private readonly Dictionary<string, object> _customFieldCache = [];
    private readonly IMessageBus _messageBus;
    private readonly Type _underlyingType;
    private bool _disposed;

    public EntityWrapper(ILDtkEntity underlyingEntity, IMessageBus messageBus)
    {
        UnderlyingEntity = underlyingEntity ?? throw new ArgumentNullException(nameof(underlyingEntity));
        _messageBus = messageBus;
        _underlyingType = underlyingEntity.GetType();

        CacheCustomFields();

        // Register with message bus if available
        _messageBus.RegisterEntity(GetEntityId(), this);
    }

    // Wrapper-specific properties
    public string EntityType => _underlyingType.Name;
    public ILDtkEntity UnderlyingEntity { get; }

    // ILDtkEntity delegation
    public string Identifier
    {
        get => UnderlyingEntity.Identifier;
        set => UnderlyingEntity.Identifier = value;
    }

    public Guid Iid
    {
        get => UnderlyingEntity.Iid;
        set => UnderlyingEntity.Iid = value;
    }

    public int Uid
    {
        get => UnderlyingEntity.Uid;
        set => UnderlyingEntity.Uid = value;
    }

    public Vector2 Position
    {
        get => UnderlyingEntity.Position;
        set => UnderlyingEntity.Position = value;
    }

    public Vector2 Size
    {
        get => UnderlyingEntity.Size;
        set => UnderlyingEntity.Size = value;
    }

    public Vector2 Pivot
    {
        get => UnderlyingEntity.Pivot;
        set => UnderlyingEntity.Pivot = value;
    }

    public Rectangle Tile
    {
        get => UnderlyingEntity.Tile;
        set => UnderlyingEntity.Tile = value;
    }

    public Color SmartColor
    {
        get => UnderlyingEntity.SmartColor;
        set => UnderlyingEntity.SmartColor = value;
    }

    // IMessageReceiver implementation
    public void ReceiveMessage<T>(T message, string? senderId = null)
        where T : class
    {
        // First try typed handlers if any behaviors implement IMessageHandler<T>
        foreach (IBehavior behavior in _behaviors)
        {
            if (behavior is IMessageHandler<T> typedHandler)
            {
                typedHandler.HandleMessage(message, senderId);
            }
        }

        // Then fallback to existing OnMessage pattern for all behaviors
        foreach (IBehavior behavior in _behaviors)
        {
            behavior.OnMessage(message);
        }
    }

    // Custom field access
    public T GetField<T>(string fieldName)
    {
        if (_customFieldCache.TryGetValue(fieldName, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        throw new ArgumentException($"Field '{fieldName}' not found or not of type {typeof(T).Name}");
    }

    public bool HasField(string fieldName)
    {
        return _customFieldCache.ContainsKey(fieldName);
    }

    public bool TryGetField<T>(string fieldName, out T? value)
    {
        if (_customFieldCache.TryGetValue(fieldName, out var obj) && obj is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default(T);
        return false;
    }

    // Behavior management
    public void AddBehavior(IBehavior? behavior)
    {
        if (behavior == null || _behaviorsByType.ContainsKey(behavior.GetType()))
        {
            return;
        }

        _behaviors.Add(behavior);
        _behaviorsByType[behavior.GetType()] = behavior;
        behavior.Initialize(this);
    }

    public T? GetBehavior<T>()
        where T : EntityBehavior
    {
        return (T?)_behaviorsByType.GetValueOrDefault(typeof(T));
    }

    public void Update(GameTime gameTime)
    {
        foreach (IBehavior behavior in _behaviors)
        {
            behavior.Update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (IBehavior behavior in _behaviors)
        {
            behavior.Draw(spriteBatch);
        }
    }

    /// <summary>
    ///     Send a local message to all behaviors on this entity (backward compatible).
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void SendMessage<T>(T message)
        where T : class
    {
        // Route through message bus with local scope for consistency
        SendMessage(message, LocalImmediateOptions);
    }

    /// <summary>
    ///     Send a message with advanced routing options.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="options">The routing options for the message.</param>
    public void SendMessage<T>(T message, MessageOptions? options)
        where T : class
    {
        if (options != null && options.Scope != MessageScope.Local)
        {
            // Delegate to message bus for non-local messages
            _messageBus.SendMessage(message, options, GetEntityId());
            return;
        }

        // Handle local messages directly
        foreach (IBehavior behavior in _behaviors)
        {
            behavior.OnMessage(message);
        }
    }

    /// <summary>
    ///     Send a message to a specific entity.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="targetEntityId">The ID of the target entity.</param>
    /// <param name="immediate">Whether to send the message immediately.</param>
    public void SendMessageToEntity<T>(T message, string targetEntityId, bool immediate = true)
        where T : class
    {
        SendMessage(
            message,
            new MessageOptions { Scope = MessageScope.Entity, TargetEntityId = targetEntityId, Immediate = immediate });
    }

    /// <summary>
    ///     Send a global message to all entities and systems.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="immediate">Whether to send the message immediately.</param>
    public void SendGlobalMessage<T>(T message, bool immediate = true)
        where T : class
    {
        SendMessage(message, new MessageOptions { Scope = MessageScope.Global, Immediate = immediate });
    }

    /// <summary>
    ///     Send a spatial message to entities within range (useful for magic effects).
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="range">The range within which to send the message.</param>
    /// <param name="position">The position from which to measure range (defaults to entity position).</param>
    /// <param name="includeSelf">Whether to include the sender in the message recipients.</param>
    /// <param name="immediate">Whether to send the message immediately.</param>
    public void SendSpatialMessage<T>(
        T message,
        float range,
        Vector2? position = null,
        bool includeSelf = false,
        bool immediate = true)
        where T : class
    {
        SendMessage(
            message,
            new MessageOptions
            {
                Scope = MessageScope.Spatial,
                Position = position ?? Position,
                Range = range,
                IncludeSender = includeSelf,
                Immediate = immediate
            });
    }

    /// <summary>
    ///     Cleanup when entity is destroyed.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    ///     Protected implementation of Dispose pattern.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Dispose managed resources
            _messageBus.UnregisterEntity(GetEntityId());

            // Cleanup behaviors
            foreach (IBehavior behavior in _behaviors)
            {
                behavior.Cleanup();
            }

            _behaviors.Clear();
            _behaviorsByType.Clear();
        }

        _disposed = true;
    }

    private void CacheCustomFields()
    {
        HashSet<string> entityInterfaceProps = [.. typeof(ILDtkEntity).GetProperties().Select(p => p.Name)];

        foreach (PropertyInfo prop in _underlyingType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (entityInterfaceProps.Contains(prop.Name) || !prop.CanRead)
            {
                continue;
            }

            var value = prop.GetValue(UnderlyingEntity);
            if (value != null)
            {
                _customFieldCache[prop.Name] = value;
            }
        }
    }

    /// <summary>
    ///     Get unique entity identifier for messaging.
    /// </summary>
    private string GetEntityId()
    {
        return $"{EntityType}_{Iid}";
    }
}
