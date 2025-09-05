using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LDtk;
using Microsoft.Xna.Framework;
using OctarineCodex.Entities.Behaviors;

public class EntityWrapper : ILDtkEntity
{
    private readonly List<IBehavior> _behaviors = new();
    private readonly Dictionary<Type, IBehavior> _behaviorsByType = new();
    private readonly Dictionary<string, object> _customFieldCache = new();
    private readonly IServiceProvider _services;
    private readonly Type _underlyingType;


    public EntityWrapper(ILDtkEntity underlyingEntity, IServiceProvider services = null)
    {
        UnderlyingEntity = underlyingEntity ?? throw new ArgumentNullException(nameof(underlyingEntity));
        _underlyingType = underlyingEntity.GetType();
        _services = services;

        CacheCustomFields();
    }

    // Wrapper-specific properties
    public string EntityType => _underlyingType.Name;
    public string SourceNamespace => _underlyingType.Namespace;
    public ILDtkEntity UnderlyingEntity { get; }

    // Common ILDtkEntity fields (delegate to underlying)
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

    // Custom field access
    public T GetField<T>(string fieldName)
    {
        if (_customFieldCache.TryGetValue(fieldName, out var value) && value is T typedValue) return typedValue;

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
        // Use reflection to find properties that aren't part of ILDtkEntity
        var entityInterfaceProps = typeof(ILDtkEntity).GetProperties().Select(p => p.Name).ToHashSet();

        foreach (var prop in _underlyingType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            if (!entityInterfaceProps.Contains(prop.Name) && prop.CanRead)
            {
                var value = prop.GetValue(UnderlyingEntity);
                _customFieldCache[prop.Name] = value;
            }
    }

    public void AddBehavior(EntityBehavior behavior)
    {
        if (_behaviorsByType.ContainsKey(behavior.GetType()))
            return; // Don't add duplicates

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

    public void SendMessage<T>(T message) where T : class
    {
        foreach (var behavior in _behaviors) behavior.OnMessage(message);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var behavior in _behaviors) behavior.Update(gameTime);
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
}