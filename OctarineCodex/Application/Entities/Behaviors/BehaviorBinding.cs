// OctarineCodex/Entities/Behaviors/BehaviorBinding.cs

using System;
using System.Collections.Generic;
using System.Linq;
using OctarineCodex.Domain.Entities;
using OctarineCodex.Domain.Magic;

namespace OctarineCodex.Application.Entities.Behaviors;

public class BehaviorBinding<T>(Func<IServiceProvider, T>? factory = null) : IBehaviorBinding
    where T : IBehavior, new()
{
    private readonly Func<IServiceProvider, T> _factory = factory ?? (_ => new T());
    private readonly List<Func<EntityWrapper, bool>> _predicates = [];
    public int Priority { get; private set; }

    public bool ShouldApply(EntityWrapper entity)
    {
        return _predicates.All(predicate => predicate(entity));
    }

    public IBehavior CreateBehavior(IServiceProvider services)
    {
        return _factory(services);
    }

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

    public BehaviorBinding<T> WithField<TValue>(string fieldName, Func<TValue?, bool> condition)
    {
        _predicates.Add(entity =>
            entity.TryGetField(fieldName, out TValue? value) && condition(value));
        return this;
    }

    public BehaviorBinding<T> WithMagicVector(EleAspects.Element element, float threshold)
    {
        _predicates.Add(entity =>
            entity.TryGetField("magicVector", out MagicSignature? vector) &&
            vector?.GetComponent(element) > threshold);
        return this;
    }

    public BehaviorBinding<T> WithPriority(int priority)
    {
        Priority = priority;
        return this;
    }
}
