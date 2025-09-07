// Updated OctarineCodex/Entities/EntityWrapper.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LDtk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OctarineCodex.Entities.Behaviors;
using OctarineCodex.Messaging;

namespace OctarineCodex.Entities;

public class EntityWrapper : ILDtkEntity, IMessageReceiver, IDisposable
{
    protected readonly List<IBehavior> _behaviors = new();
    private readonly Dictionary<Type, IBehavior> _behaviorsByType = new();
    private readonly Dictionary<string, object> _customFieldCache = new();
    private readonly IMessageBus? _messageBus;
    private readonly IServiceProvider _services;
    private readonly Type _underlyingType;

    public EntityWrapper(ILDtkEntity underlyingEntity, IServiceProvider services = null)
    {
        UnderlyingEntity = underlyingEntity ?? throw new ArgumentNullException(nameof(underlyingEntity));
        _underlyingType = underlyingEntity.GetType();
        _services = services;
        _messageBus = services?.GetRequiredService<IMessageBus>();

        CacheCustomFields();

        // Register with message bus if available
        if (_messageBus != null) _messageBus.RegisterEntity(GetEntityId(), this);
    }

    // Wrapper-specific properties
    public string EntityType => _underlyingType.Name;
    public string SourceNamespace => _underlyingType.Namespace;
    public ILDtkEntity UnderlyingEntity { get; }

    // ILDtkEntity delegation (same as before)
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
    public void ReceiveMessage<T>(T message, string? senderId = null) where T : class
    {
        // First try typed handlers if any behaviors implement IMessageHandler<T>
        foreach (var behavior in _behaviors)
            if (behavior is IMessageHandler<T> typedHandler)
                typedHandler.HandleMessage(message, senderId);

        // Then fallback to existing OnMessage pattern for all behaviors
        foreach (var behavior in _behaviors) behavior.OnMessage(message);
    }

    // Custom field access (same as before)
    public T GetField<T>(string fieldName)
    {
        if (_customFieldCache.TryGetValue(fieldName, out var value) && value is T typedValue)
            return typedValue;

        throw new ArgumentException($"Field '{fieldName}' not found or not of type {typeof(T).Name}");
    }

    public bool HasField(string fieldName)
    {
        return _customFieldCache.ContainsKey(fieldName);
    }

    public bool TryGetField<T>(string fieldName, out T value)
    {
        if (_customFieldCache.TryGetValue(fieldName, out var obj) && obj is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }

    public IReadOnlyDictionary<string, object> GetAllCustomFields()
    {
        return _customFieldCache.AsReadOnly();
    }

    private void CacheCustomFields()
    {
        var entityInterfaceProps = typeof(ILDtkEntity).GetProperties().Select(p => p.Name).ToHashSet();

        foreach (var prop in _underlyingType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            if (!entityInterfaceProps.Contains(prop.Name) && prop.CanRead)
            {
                var value = prop.GetValue(UnderlyingEntity);
                _customFieldCache[prop.Name] = value;
            }
    }

    // Behavior management (same as before)
    public void AddBehavior(EntityBehavior behavior)
    {
        if (_behaviorsByType.ContainsKey(behavior.GetType()))
            return;

        _behaviors.Add(behavior);
        _behaviorsByType[behavior.GetType()] = behavior;
        behavior.Initialize(this, _services);
    }

    public T GetBehavior<T>() where T : EntityBehavior
    {
        return (T)_behaviorsByType.GetValueOrDefault(typeof(T));
    }

    public bool HasBehavior<T>() where T : EntityBehavior
    {
        return _behaviorsByType.ContainsKey(typeof(T));
    }

    public void RemoveBehavior<T>() where T : EntityBehavior
    {
        var behaviorType = typeof(T);
        if (_behaviorsByType.TryGetValue(behaviorType, out var behavior))
        {
            behavior.Cleanup();
            _behaviors.Remove(behavior);
            _behaviorsByType.Remove(behaviorType);
        }
    }

    // Enhanced messaging system

    /// <summary>
    ///     Send a local message to all behaviors on this entity (backward compatible)
    /// </summary>
    public void SendMessage<T>(T message) where T : class
    {
        // Route through message bus with local scope for consistency
        SendMessage(message, new MessageOptions { Scope = MessageScope.Local, Immediate = true });
    }

    /// <summary>
    ///     Send a message with advanced routing options
    /// </summary>
    public void SendMessage<T>(T message, MessageOptions options) where T : class
    {
        if (_messageBus == null)
        {
            // Fallback to local messaging if no message bus available
            if (options.Scope == MessageScope.Local) SendMessage(message);
            return;
        }

        _messageBus.SendMessage(message, options, GetEntityId());
    }

    /// <summary>
    ///     Send a message to a specific entity
    /// </summary>
    public void SendMessageToEntity<T>(T message, string targetEntityId, bool immediate = true) where T : class
    {
        SendMessage(message, new MessageOptions
        {
            Scope = MessageScope.Entity,
            TargetEntityId = targetEntityId,
            Immediate = immediate
        });
    }

    /// <summary>
    ///     Send a global message to all entities and systems
    /// </summary>
    public void SendGlobalMessage<T>(T message, bool immediate = true) where T : class
    {
        SendMessage(message, new MessageOptions
        {
            Scope = MessageScope.Global,
            Immediate = immediate
        });
    }

    /// <summary>
    ///     Send a spatial message to entities within range (useful for magic effects)
    /// </summary>
    public void SendSpatialMessage<T>(T message, float range, Vector2? position = null, bool includeSelf = false,
        bool immediate = true) where T : class
    {
        SendMessage(message, new MessageOptions
        {
            Scope = MessageScope.Spatial,
            Position = position ?? Position,
            Range = range,
            IncludeSender = includeSelf,
            Immediate = immediate
        });
    }

    public virtual void Update(GameTime gameTime)
    {
        foreach (var behavior in _behaviors) behavior.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var behavior in _behaviors) behavior.Draw(spriteBatch);
    }

    /// <summary>
    ///     Get unique entity identifier for messaging
    /// </summary>
    public string GetEntityId()
    {
        return $"{EntityType}_{Iid}";
    }

    /// <summary>
    ///     Cleanup when entity is destroyed
    /// </summary>
    public void Dispose()
    {
        // Unregister from message bus
        if (_messageBus != null) _messageBus.UnregisterEntity(GetEntityId());

        // Cleanup behaviors
        foreach (var behavior in _behaviors) behavior.Cleanup();

        _behaviors.Clear();
        _behaviorsByType.Clear();
    }
}