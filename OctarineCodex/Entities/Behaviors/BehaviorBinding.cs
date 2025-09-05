// OctarineCodex/Entities/Behaviors/BehaviorBinding.cs

using System;
using System.Collections.Generic;
using System.Linq;
using OctarineCodex.Magic;

namespace OctarineCodex.Entities.Behaviors;

public class BehaviorBinding<T> : IBehaviorBinding where T : IBehavior, new()
{
    private readonly Func<IServiceProvider, T> _factory;
    private readonly List<Func<EntityWrapper, bool>> _predicates = new();

    public BehaviorBinding(Func<IServiceProvider, T> factory = null)
    {
        _factory = factory ?? (services => new T());
    }

    public bool ShouldApply(EntityWrapper entity)
    {
        return _predicates.All(predicate => predicate(entity));
    }

    public IBehavior CreateBehavior(IServiceProvider services)
    {
        return _factory(services);
    }

    public int Priority { get; private set; }

    public BehaviorBinding<T> ForEntityType(string entityType)
    {
        _predicates.Add(entity => entity.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase));
        return this;
    }

    public BehaviorBinding<T> WithField(string fieldName)
    {
        _predicates.Add(entity => entity.HasField(fieldName));
        return this;
    }

    public BehaviorBinding<T> WithField<TValue>(string fieldName, Func<TValue, bool> condition)
    {
        _predicates.Add(entity =>
            entity.TryGetField<TValue>(fieldName, out var value) && condition(value));
        return this;
    }

    public BehaviorBinding<T> WithMagicVector(EleAspects.Element element, float threshold)
    {
        _predicates.Add(entity =>
            entity.TryGetField<Signature>("magicVector", out var vector) &&
            vector.GetComponent(element) > threshold);
        return this;
    }

    public BehaviorBinding<T> WithPriority(int priority)
    {
        Priority = priority;
        return this;
    }
}

public static class BehaviorBinding
{
    public static BehaviorBinding<T> Create<T>(Func<IServiceProvider, T> factory = null)
        where T : IBehavior, new()
    {
        return new BehaviorBinding<T>(factory);
    }
}